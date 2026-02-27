from bisect import bisect_right
from collections import defaultdict
from os import listdir
from os.path import isfile, join
from sys import exception
import typing
import msgpack
import numpy as np
from setuptools.namespaces import flatten
from torchgen.api.cpp import returns_type

from Layout import *
import torch
from torch.utils.data import Dataset

import json




class BossFightDataset(Dataset):

    _data_batches: list[DataBatch]
    _data_dir: str
    _cumulative_indexes: list[int]
    _boss_playmaker_indexes: dict[str, dict[str, int]]
    _len: int

    def __init__(self, data_dir, expected_format_version, use_only_successful = True):
        self._data_dir = data_dir

        print(f"loading dataset from directory: {data_dir}")
        self._data_batches = get_filtered_batches(data_dir, expected_format_version, use_only_successful)
        print(f"Found {len(self._data_batches)} data files that match the expected format")

        print("generating indexes for boss playmakers")
        self._boss_playmaker_indexes = get_boss_playmaker_indexes(data_dir, expected_format_version)

        self._cumulative_indexes = [0]
        self._len = 0
        for data_batch in self._data_batches:
            self._cumulative_indexes.append(self._cumulative_indexes[-1] + data_batch.frame_count)
            self._len = self._len + data_batch.frame_count

        print(self._cumulative_indexes)

    def __len__(self):
        return self._len

    def __getitem__(self, idx):
        if idx < 0 or idx >= len(self):
            raise IndexError("list index out of range")

        file_idx = bisect_right(self._cumulative_indexes, idx) - 1
        frame_idx = idx - self._cumulative_indexes[file_idx]

        with open(join(self._data_dir, self._data_batches[file_idx].data_filename), "rb") as f:
            unpacker = msgpack.Unpacker(f, raw=False)

            for frame in unpacker:
                frame[0][6]:list[list[str, str]]
                frame[0].pop(6) # remove playmakers
                frame.pop(3) # remove outputs

                print(frame)

                data = np.concatenate(frame)


                print(data)











if __name__ == '__main__':
    dataset = BossFightDataset("../datasets/Lace Boss1", SUPPORTED_FORMAT_VERSION, True)

    print(dataset[100])
