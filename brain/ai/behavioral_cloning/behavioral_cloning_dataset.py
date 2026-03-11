import bisect
import itertools
from pathlib import Path
from pprint import pprint

import torch
from torch.utils.data import Dataset

from ai.models import get_artifact_training_dataset_path
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
        }

# if __name__ == "__main__":
#     d = BossFightDataset(get_artifact_training_dataset_path("Lace Boss1", "temp"), 5)
#
#     pprint(d[0]["input_named_state"])
#     pprint(d[1]["input_named_state"])
#     pprint(d[2]["input_named_state"])
#     pprint(d[3]["input_named_state"])
#     pprint(d[4]["input_named_state"])