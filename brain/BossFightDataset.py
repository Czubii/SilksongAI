import torch
from torch.utils.data import Dataset


class BossFightDataset(Dataset):
    def __init__(self, data, time_window: int = 5):
        """
        :param processed_data_filename:
        :param time_window: number of previous frames that gets returned in getitem together with the one asked for
        """
        self._time_window = time_window
        self._cont = data["continuous"]
        self._playmakers = data["playmakers"]
        self._targets = data["targets"]
        self._frame_counts = data["frame_counts"]

    def __len__(self):
        return self._cont.shape[0] - self._time_window

    def __getitem__(self, idx):
        return (self._cont[idx : idx+self._time_window],
                self._playmakers[idx : idx+self._time_window],
                self._targets[idx+self._time_window-1])
