import os
from dataclasses import asdict
from math import ceil
from pathlib import Path
from typing import Optional

import numpy as np
import torch

from shared import ModelDimensions
from .layout import NamedState, SUPPORTED_LAYOUT_VERSION
from .frame_processing import FrameProcessor
from .recording_reader import RecordingFilters, RecordingReader
from .statistics import OnlineSampleStatistics
from .vocabulary import Vocabulary


class RecordingProcessor:
    def __init__(self, data_path: str, target_boss: str, filters: RecordingFilters):

        self._data_reader = RecordingReader(data_path, target_boss, filters)

        if self._data_reader.get_recording_count() < 2:
            raise Exception("At least 2 recordings are required")

        self._target_boss = target_boss

        self._vocabulary = Vocabulary()
        self._vocabulary.from_recordings(self._data_reader)

        self._dims = self._evaluate_dims()

        self._statistics = OnlineSampleStatistics([self._dims.input_continuous])
        self._statistics.from_recordings(self._data_reader)

        self._frame_processor = FrameProcessor(self._vocabulary, self._statistics, self._dims)

    def _evaluate_dims(self) -> ModelDimensions:
        '''
        verifies whether all recording frames have the same dimensions and returns those dimensions
        :return:
        '''
        expected_cont_dim = None
        expected_bool_dim = None
        expected_ns_dim = None

        expected_target_cont_dim = None
        expected_target_bool_dim = None

        for frame, _ in self._data_reader.frames():
            cont_dim = frame.FrameData.continuous_count()
            ns_dim = frame.FrameData.type_count(NamedState)
            bool_dim = frame.FrameData.boolean_count()

            target_cont_dim = frame.Targets.continuous_count()
            target_bool_dim = frame.Targets.boolean_count()

            if expected_bool_dim is None:
                expected_bool_dim = bool_dim
            elif bool_dim != expected_bool_dim:
                raise Exception(f"ProcessingPipeline: Bool vector length mismatch! Got {bool_dim}, expected {expected_bool_dim}")

            if expected_cont_dim is None:
                expected_cont_dim = cont_dim
            elif cont_dim != expected_cont_dim:
                raise Exception(f"ProcessingPipeline: Continuous vector length mismatch! Got {cont_dim}, expected {expected_cont_dim}")

            if expected_ns_dim is None:
                expected_ns_dim = ns_dim
            elif ns_dim != expected_ns_dim:
                raise Exception(f"ProcessingPipeline: named state vector length mismatch! Got {ns_dim}, expected {expected_ns_dim}")

            if expected_target_cont_dim is None:
                expected_target_cont_dim = target_cont_dim
            elif target_cont_dim != expected_target_cont_dim:
                raise Exception(f"ProcessingPipeline: target cont vector length mismatch! Got {target_cont_dim}, expected {expected_target_cont_dim}")

            if expected_target_bool_dim is None:
                expected_target_bool_dim = target_bool_dim
            elif target_bool_dim != expected_target_bool_dim:
                raise Exception(f"ProcessingPipeline: target bool vector length mismatch! Got {target_bool_dim}, expected {expected_target_bool_dim}")

        print(f"ProcessingPipeline: Recording verification passed")

        return ModelDimensions(expected_cont_dim,
                               expected_bool_dim,
                               expected_ns_dim,
                               expected_target_cont_dim,
                               expected_target_bool_dim,
                               len(self._vocabulary))

    def process_and_save(self,
                         output_dir: Path,
                         testing_count: Optional[int] = None,
                         testing_percent: Optional[float] = None) -> None:
        """
        :param output_dir: self-explanatory
        :param testing_percent: number of recordings used for testing
        :param testing_count: percent of recordings used for testing
        """
        if (testing_count is not None) and (testing_percent is not None):
            raise ValueError("Provide 'count' OR 'percent', not both.")
        if (testing_count is None) and (testing_percent is None):
            raise ValueError("You must provide either 'count' or 'percent'.")

        if testing_count is None:
            testing_count = ceil(testing_percent * self._data_reader.get_recording_count())

        testing_count = max(1, min(testing_count, self._data_reader.get_recording_count()-1))
        frame_counts = self._data_reader.get_frame_counts()
        testing_frame_count = sum(frame_counts[:testing_count])

        total_frames = self._data_reader.get_frame_count()

        cont_list = np.zeros([total_frames, self._dims.input_continuous], dtype=np.float32)
        bool_list = np.zeros([total_frames, self._dims.input_boolean], dtype=np.float32)
        named_state_idx_list = np.zeros([total_frames, self._dims.input_named_state], dtype=np.float32)

        target_cont_list = np.zeros([total_frames, self._dims.output_continuous], dtype=np.float32)
        target_bool_list = np.zeros([total_frames, self._dims.output_boolean], dtype=np.float32)

        prev_id = -1
        for i, (frame, recording_info) in enumerate(self._data_reader.frames()):
            current_id = recording_info.get("recording_id")
            if current_id != prev_id:
                prev_id = current_id

            cont, boolean, named_state_idx = self._frame_processor.process_frame(frame.FrameData)
            cont_list[i, :] = cont
            bool_list[i, :] = boolean
            named_state_idx_list[i, :] = named_state_idx

            target_cont_list[i, :] = frame.Targets.get_all_continuous()
            target_bool_list[i, :] = frame.Targets.get_all_booleans()

        target_bool = np.array(target_bool_list, dtype=bool)

        testing_bool = target_bool[:testing_frame_count]
        training_bool = target_bool[testing_frame_count:]

        testing_bool_counts_pos = np.sum(testing_bool, axis=0, dtype=np.int32)
        training_bool_counts_pos = np.sum(training_bool, axis=0, dtype=np.int32)

        testing_bool_counts_neg = np.sum(~testing_bool, axis=0, dtype=np.int32)
        training_bool_counts_neg = np.sum(~training_bool, axis=0, dtype=np.int32)

        data_test = {
            "layout_version": SUPPORTED_LAYOUT_VERSION,
            "target_boss": self._target_boss,
            "frame_counts": frame_counts[:testing_count],

            "input_continuous": torch.tensor(cont_list[:testing_frame_count]),
            "input_boolean": torch.tensor(bool_list[:testing_frame_count]),
            "input_named_state": torch.tensor(named_state_idx_list[:testing_frame_count]),

            "output_continuous": torch.tensor(target_cont_list[:testing_frame_count]),
            "output_boolean": torch.tensor(target_bool_list[:testing_frame_count]),

            "output_boolean_counts_pos": torch.tensor(testing_bool_counts_pos),
            "output_boolean_counts_neg": torch.tensor(testing_bool_counts_neg),

            "dims": asdict(self._dims),
            "frame_processor": self._frame_processor.to_dict()
        }
        torch.save(data_test, os.path.join(output_dir, f"testing_data.pt"))

        data_train = {
            "layout_version": SUPPORTED_LAYOUT_VERSION,
            "target_boss": self._target_boss,
            "frame_counts": frame_counts[testing_count:],

            "input_continuous": torch.tensor(cont_list[testing_frame_count:]),
            "input_boolean": torch.tensor(bool_list[testing_frame_count:]),
            "input_named_state": torch.tensor(named_state_idx_list[testing_frame_count:]),

            "output_continuous": torch.tensor(target_cont_list[testing_frame_count:]),
            "output_boolean": torch.tensor(target_bool_list[testing_frame_count:]),

            "output_boolean_counts_pos": torch.tensor(training_bool_counts_pos),
            "output_boolean_counts_neg": torch.tensor(training_bool_counts_neg),

            "dims": asdict(self._dims),
            "frame_processor": self._frame_processor.to_dict()
        }
        torch.save(data_train, os.path.join(output_dir, f"training_data.pt"))