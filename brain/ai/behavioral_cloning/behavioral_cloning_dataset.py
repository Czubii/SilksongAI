import bisect
import itertools
from pathlib import Path

import torch
from torch.utils.data import Dataset

from data_processing.layout import SUPPORTED_LAYOUT_VERSION, LayoutNotSupported


class BossFightDataset(Dataset):
    def __init__(self, dataset_file_path: Path, time_window: int = 2):

        data = torch.load(dataset_file_path, weights_only=False)

        if data["layout_version"] != SUPPORTED_LAYOUT_VERSION:
            raise LayoutNotSupported(f"Dataset layout version not supported. Dataset version: {data["layout_version"]}")

        self.time_window = time_window
        self.target_boss = data["target_boss"]

        self.input_continuous = data["input_continuous"]
        self.input_boolean = data["input_boolean"]
        self.input_named_state = data["input_named_state"]
        self.output_continuous = data["output_continuous"]
        self.output_boolean = data["output_boolean"]

        self.frame_counts = data["frame_counts"]

        self.returns = data["returns"]

        self.output_boolean_counts_pos = data["output_boolean_counts_pos"]
        self.output_boolean_counts_neg = data["output_boolean_counts_neg"]
        self.output_boolean_counts_pos[self.output_boolean_counts_pos==0] = 1
        self.pos_weights = self.output_boolean_counts_neg/self.output_boolean_counts_pos

        self._windows_per_recording = []
        for count in self.frame_counts:
            self._windows_per_recording.append(max(0, count - (self.time_window - 1)))

        self._accumulated_windows = list(itertools.accumulate(self._windows_per_recording))

        self._len = sum(self._windows_per_recording)


    def __len__(self):
        return self._len

    def __getitem__(self, idx) -> dict:

        recording_idx = bisect.bisect_right(self._accumulated_windows, idx)

        end_global_idx = idx + (recording_idx+1) * (self.time_window - 1)
        start_global_idx = end_global_idx - self.time_window + 1
        return {
            "input_continuous": self.input_continuous[start_global_idx:end_global_idx+1],
            "input_boolean": self.input_boolean[start_global_idx:end_global_idx+1],
            "input_named_state": self.input_named_state[start_global_idx:end_global_idx+1],
            "output_continuous": self.output_continuous[end_global_idx],
            "output_boolean": self.output_boolean[end_global_idx],
            "returns": self.returns[end_global_idx],
        }
    