from typing import List

import numpy as np

from .recording_reader import RecordingReader


class OnlineSampleStatistics: #
    """
    Based on Welford's online algorithm https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
    """

    def __init__(self,
                 dim: List[int],
                 mean: np.ndarray = None,
                 M2: np.ndarray = None,
                 count: np.ndarray = None):

        self._dim = dim
        self._mean = mean if mean is not None else np.zeros(dim, dtype=np.float64)
        self._M2 = M2 if M2 is not None else np.zeros(dim, dtype=np.float64)
        self._count = count if count is not None else 0

    def from_recordings(self, reader: RecordingReader):
        for frame, _ in reader.frames():
            self.add_vector(np.array(frame.FrameData.get_all_continuous()))

    def add_vector(self, x: np.ndarray):
        self._count += 1
        old_mean = self._mean
        self._mean += (x - old_mean) / self._count
        self._M2 += (x - old_mean) * (x - self._mean)

    def get_mean(self):
        return self._mean

    def get_sample_variance(self) -> np.ndarray:
        return self._M2 / (self._count - 1)

    def get_sample_std(self) -> np.ndarray:
        return np.sqrt(self.get_sample_variance())

    def to_dict(self) -> dict:
        return {
            "dim": self._dim,
            "mean": self._mean,
            "M2": self._M2,
            "count": self._count
        }

    @classmethod
    def from_dict(cls, d: dict):
        return cls(**d)
