import os
from dataclasses import dataclass, asdict
from math import ceil
from pprint import pprint
from typing import Tuple, Optional
import numpy as np
import torch

from data_processing.layout import FrameData, NamedStatesContainer, NamedState, UserInputs, SUPPORTED_LAYOUT_VERSION
from data_processing.statistics import OnlineSampleStatistics
from model_dimensions import ModelDimensions
from data_processing.recording_reader import RecordingReader, RecordingFilters, ChoosePercentBest
from data_processing.vocabulary import Vocabulary

class FrameProcessor:
    def __init__(self, vocabulary: Vocabulary, statistics: OnlineSampleStatistics, dims: ModelDimensions):
        self._vocabulary = vocabulary
        self._statistics = statistics
        self._dims = dims

        self._cont_buffer = np.zeros(dims.input_continuous, dtype=np.float32)
        self._bool_buffer = np.zeros(dims.input_boolean, dtype=np.float32)
        self._named_state_idx_buffer = np.zeros(dims.input_named_state, dtype=np.int32)

        self._mean = self._statistics.get_mean()
        self._std = self._statistics.get_sample_std()
        self._std[self._std == 0] = 1.0

    def update_statistics(self):
        self._mean = self._statistics.get_mean()
        self._std = self._statistics.get_sample_std()
        self._std[self._std == 0] = 1.0

    def process_frame(self, frame: FrameData) -> Tuple[np.ndarray, np.ndarray, np.ndarray]:
        '''
        :return: arrays in order: continuous, boolean, named-state idx-s
        '''

        self._cont_buffer = np.array(frame.get_all_continuous(), dtype=np.float32)
        self._cont_buffer -= self._mean
        self._cont_buffer /= self._std

        self._bool_buffer = np.array(frame.get_all_booleans(), dtype=np.float32)

        containers = frame.get_all_of_type([NamedStatesContainer])

        self._named_state_idx_buffer = np.array([
            self._vocabulary.get(container.ParentName, named_state.Name, named_state.StateName) for container in containers
            for named_state in container.NamedStates
        ],  dtype=np.int32)

        return self._cont_buffer, self._bool_buffer, self._named_state_idx_buffer

    def to_dict(self) -> dict:
        return {
            "vocabulary": self._vocabulary.to_dict(),
            "statistics": self._statistics.to_dict(),
            "dims": asdict(self._dims)
        }

    @classmethod
    def from_dict(cls, data: dict):
        # This uses the type hints in __init__ to figure out how to cast the dicts
        return cls(
            vocabulary=Vocabulary.from_dict(data["vocabulary"]),
            statistics=OnlineSampleStatistics.from_dict(data["statistics"]),
            dims=ModelDimensions(**data["dims"])
        )

class ProcessingPipeline:
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
            ns_dim = frame.FrameData.type_count([NamedState])
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

    def process(self,
                output_dir: str,
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

        testing_bool_counts = np.sum(target_bool_list[:testing_frame_count], axis=0, dtype=np.int32)
        training_bool_counts = np.sum(target_bool_list[testing_frame_count:], axis=0, dtype=np.int32)

        data_test = {
            "layout_version": SUPPORTED_LAYOUT_VERSION,
            "target_boss": self._target_boss,
            "frame_counts": frame_counts[:testing_count],

            "input_continuous": torch.tensor(cont_list[:testing_frame_count]),
            "input_boolean": torch.tensor(bool_list[:testing_frame_count]),
            "input_named_state": torch.tensor(named_state_idx_list[:testing_frame_count]),

            "output_continuous": torch.tensor(target_cont_list[:testing_frame_count]),
            "output_boolean": torch.tensor(target_bool_list[:testing_frame_count]),
            "output_boolean_counts": torch.tensor(testing_bool_counts),

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
            "output_boolean_counts": torch.tensor(training_bool_counts),

            "dims": asdict(self._dims),
            "frame_processor": self._frame_processor.to_dict()
        }
        torch.save(data_train, os.path.join(output_dir, f"training_data.pt"))