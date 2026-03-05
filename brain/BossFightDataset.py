import torch
from torch.utils.data import Dataset

from RecordingLayout import Layout, LayoutNotSupported


class BossFightDataset(Dataset):
    def __init__(self, data, time_window: int = 5):
        if data["layout_version"] != Layout.SUPPORTED_FORMAT_VERSION:
            raise LayoutNotSupported(
                "BehaviorCloningTrainer: Training dataset uses different layout format than currently supported")

        self.time_window = time_window
        self.target_boss = data["target_boss"]
        self.cont = data["continuous"]
        self.playmakers = data["playmakers"]
        self.targets = data["targets"]
        self.frame_counts = data["frame_counts"]

        # recording ranges
        self._recording_ranges = []
        start = 0
        for count in self.frame_counts:
            end = start + count
            self._recording_ranges.append((start, end))
            start = end

        # precompute valid window counts per recording
        self._windows_per_recording = [
            max(0, count - self.time_window + 1)
            for count in self.frame_counts
        ]

        self._length = sum(self._windows_per_recording)

    def __len__(self):
        return self._length - 5

    def __getitem__(self, idx):
        # find which recording this idx belongs to
        rec_idx = 0
        while idx >= self._windows_per_recording[rec_idx]:
            idx -= self._windows_per_recording[rec_idx]
            rec_idx += 1

        start, _ = self._recording_ranges[rec_idx]
        real_idx = start + idx

        cont_window = self.cont[real_idx: real_idx + self.time_window]
        playmakers_window = self.playmakers[real_idx: real_idx + self.time_window]
        target = self.targets[real_idx + self.time_window - 1]

        return cont_window, playmakers_window, target