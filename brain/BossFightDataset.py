import torch
from torch.utils.data import Dataset


class BossFightDataset(Dataset):
    def __init__(self, processed_data_filename: str, time_window: int = 5):
        """
        :param processed_data_filename:
        :param time_window: number of previous frames that gets returned in getitem together with the one asked for
        """
        self._time_window = time_window
        data = torch.load(f"processed/{processed_data_filename}")
        self._cont = data["continuous"]
        self._playmakers = data["playmakers"]
        self._targets = data["targets"]

    def __len__(self):
        return self._cont.shape[0] - self._time_window

    def __getitem__(self, idx):
        return (self._cont[idx : idx+self._time_window],
                self._playmakers[idx : idx+self._time_window],
                self._targets[idx+self._time_window-1])


if __name__ == "__main__":
    dataset = BossFightDataset(processed_data_filename="mossbone_mother.pt")
    print(dataset[0])