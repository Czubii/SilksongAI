from dataclasses import asdict
from typing import Tuple
import numpy as np

from shared import ModelDimensions
from .layout import FrameData, NamedStatesContainer
from .statistics import OnlineSampleStatistics
from .vocabulary import Vocabulary

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