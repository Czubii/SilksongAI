import torch
from torch.utils.data import Dataset

from RecordingLayout import Layout, LayoutNotSupported


class BossFightDataset(Dataset):
    def __init__(self, data, time_window: int = 5):
        """
        :param data: dictionary with 'continuous', 'playmakers', 'targets', 'frame_counts'
        :param time_window: number of previous frames returned along with the current one
        """
        if data["layout_version"] != Layout.SUPPORTED_FORMAT_VERSION:
            raise LayoutNotSupported(
                "BehaviorCloningTrainer: Training dataset uses different layout format than currently supported")

        self.time_window = time_window
        self.target_boss = data["target_boss"]
        self.cont = data["continuous"]
        self.playmakers = data["playmakers"]
        self.targets = data["targets"]
        self.frame_counts = data["frame_counts"]
        self.num_recordings = len(self.frame_counts)

        # Compute the start and end indices for each recording
        self._recording_ranges = []
        start = 0
        for count in self.frame_counts:
            end = start + count
            self._recording_ranges.append((start, end))
            start = end

        # Total valid indices that allow full time_window
        self._valid_indices = []
        for start, end in self._recording_ranges:
            # Only frames where we can take a full time_window
            for i in range(start, end - self.time_window + 1):
                self._valid_indices.append(i)

    def __len__(self):
        return len(self._valid_indices)

    def __getitem__(self, idx):
        real_idx = self._valid_indices[idx]
        cont_window = self.cont[real_idx: real_idx + self.time_window]
        playmakers_window = self.playmakers[real_idx: real_idx + self.time_window]
        target = self.targets[real_idx + self.time_window - 1]
        return cont_window, playmakers_window, target