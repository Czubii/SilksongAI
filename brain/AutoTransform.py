from collections import defaultdict
from os.path import join

import msgpack
import numpy as np

from DataReader import get_filtered_batches
from Layout import *


class AutoTransform:
    def __init__(self):
        self._min_values = np.array([np.inf]*23)
        self._max_values = np.array([-np.inf]*23)
        self._ranges = np.array([])
        self._playmaker_indexes: dict[str, dict[str, int]] = None
        return

    def fit(self, data_dir, expected_format_version):
        batches = get_filtered_batches(data_dir, expected_format_version, False)

        self._playmaker_indexes = get_boss_playmaker_indexes(data_dir, expected_format_version)

        for batch in batches:

            with open(join(data_dir, batch.data_filename), "rb") as f:
                unpacker = msgpack.Unpacker(f, raw=False)

                for frame in unpacker:
                    frame[0].pop(6)  # remove playmakers
                    frame.pop(3)  # remove outputs

                    data = np.concatenate(frame)

                    self._min_values = np.minimum(data, self._min_values)
                    self._max_values = np.maximum(data, self._max_values)

        self._ranges = self._max_values - self._min_values

    def transform (self, raw_frame):
        return (data - self._min_values) / self._ranges


def get_boss_playmaker_indexes(dataset_dir, expected_format_version) -> dict[str, dict[str, int]]:

    #TODO add the same for enemies
    all_session_files = get_filtered_batches(dataset_dir, expected_format_version, False)

    playmaker_pairs = (
        (pm.name, pm.state_name)
        for session_files in all_session_files
        for raw_frame in msgpack.Unpacker(open(join(dataset_dir, session_files.data_filename), "rb"), raw=False)
        for pm in FrameData.from_raw(raw_frame).boss.playmakers
    )
    boss_playmakers_raw = defaultdict(set)
    for name, state_name in playmaker_pairs:
        boss_playmakers_raw[name].add(state_name)

    boss_playmakers_indexed: dict[str, dict[str, int]] = {
        name: {state: i for i, state in enumerate(sorted(states))}
        for name, states in boss_playmakers_raw.items()
    }

    return boss_playmakers_indexed

if __name__ == '__main__':
    transform = AutoTransform()

    transform.fit("../datasets/Lace Boss1", SUPPORTED_FORMAT_VERSION)