import json
import re
from collections import defaultdict
from dataclasses import dataclass
from os import listdir
from os.path import isfile, join
from typing import Iterable, overload, Iterator, Tuple
import msgpack
import numpy as np
from torch import NoneType, layout

from Layout import *


@dataclass
class DatasetFile:
    filename: str
    info: dict

class RawDatasetReader:
    def __init__(self, data_path: str, target_boss: str):
        self._data_path = data_path
        self.target_boss = target_boss
        self.usable_files = self.get_filtered_dataset_files()
        if len(self.usable_files) == 0: raise Exception("No Usable Files Found!!!")


    def get_filtered_dataset_files(self) -> list[DatasetFile]:
        filenames = [f for f in listdir(self._data_path) if isfile(join(self._data_path, f))]

        dataset_files: list[DatasetFile] = []
        # group the info and data files together, make sure we have both .info and .msgpack files:
        for filename in filenames:
            if not filename.endswith(".msgpack"): continue

            with (open(join(self._data_path, filename), "rb") as f):
                unpacker = msgpack.Unpacker(f, raw=False)
                try:
                    header = next(unpacker)
                except StopIteration:
                    continue
                format_version = header.get("Format Version", int)

                if format_version != Layout.SUPPORTED_FORMAT_VERSION or header.get("BossName") != self.target_boss:
                    continue
                #TODO: add some checking whether in all recordings the number of enemies is constant!!!!
                footer = None
                for obj in unpacker:
                    footer = obj

                info = dict(header)
                info.update(footer)

                dataset_files.append(DatasetFile(
                    filename=filename,
                    info=info,
                ))

        return dataset_files

    def get_enemy_count(self) -> int:
        return len(self.usable_files[0].info["EnemyNames"])

    def iterate_frames(self, require_success) -> Iterable[Tuple[list[any], dict]]:
        for data_file in self.usable_files:
            if require_success and data_file.info["Success"] != True: continue

            with open(join(self._data_path, data_file.filename), "rb") as f:
                unpacker = msgpack.Unpacker(f, raw=False)
                try:
                    header = next(unpacker)
                except StopIteration:
                    continue

                prev_frame = None
                for frame in unpacker:
                    if prev_frame is not None:
                        yield prev_frame, data_file.info
                    prev_frame = frame


class PlaymakerIndexer: #TODO: add _dictionary saving
    def __init__(self, reader: RawDatasetReader):
        self._reader = reader
        self._vocab: set[str] = set()
        self._dictionary: dict[str, int] = dict()
        self._indexed = False

        self._instance_regex = re.compile(r' \(\d+\)$')

    def run(self):

        # playmakers_raw = defaultdict(lambda: defaultdict(set)) a bit different approach to indexing - #TODO: take a closer look at it in the future
        # for enemy_name, playmaker_name, state_name in self._get_playmaker_pairs_per_enemy():
        #     playmakers_raw[enemy_name][playmaker_name].add(state_name)
        #
        # boss_playmakers_indexed: dict[str, dict[str, dict[str, int]]] = {
        #     name: {state: i for i, state in enumerate(sorted(states))}
        #     for name, states in boss_playmakers_raw.items()
        # }

        # Very important - this cuts out the instance number ((1),(2) etc.) when later doing anything with this data this has to be taken into account
        for enemy_name, playmaker_name, state_name in self._get_playmaker_pairs_per_enemy():
            self._vocab.add(f"{re.sub(self._instance_regex, '', enemy_name)}|{playmaker_name}|{state_name}")

        for i, word in enumerate(sorted(self._vocab)):
            self._dictionary[word] = i

        self._indexed = True

    def _get_playmaker_pairs_per_enemy(self) -> Iterable[tuple[str, str, str]]:
        for frame, recording_info in self._reader.iterate_frames(require_success=False):
            enemy_list = frame[Layout.Frame.ENEMIES]
            if not enemy_list:
                continue

            for enemy_data, enemy_name in zip(enemy_list, recording_info["EnemyNames"]):
                if enemy_data is None:
                    continue

                pms = (enemy_data[Layout.Enemy.PLAY_MAKERS] or [["", ""]])  # no playmakers -> add placeholder token #TODO rethink in foreseeable future
                for playmaker in pms:
                    yield enemy_name, playmaker[Layout.Playmaker.NAME], playmaker[Layout.Playmaker.STATE_NAME]

    def get(self, enemy_name: str, playmaker_name: str, state_name:str) -> int:
        if not self._indexed:
            raise Exception("Not indexed! You have to call run() first!")
        return self._dictionary[f"{re.sub(self._instance_regex, '', enemy_name)}|{playmaker_name}|{state_name}"]


class Preprocessor:
    def __init__(self, reader: RawDatasetReader, require_success=True):
        self._reader = reader
        self._pm_indexer = PlaymakerIndexer(self._reader)
        self._pm_indexer.run()
        self._mean = self._get_mean_of_cont()
        self._std = self._get_std_of_cont()
        self._require_success = require_success


    @staticmethod
    def _build_continuous_vector(frame) -> np.ndarray:
        return np.array([
            *[val for val in frame[Layout.Frame.HERO]],
            *[val for enemy in frame[Layout.Frame.ENEMIES]
              for val in enemy[:Layout.Enemy.PLAY_MAKERS]]
        ], dtype=np.float32)

    def _build_playmaker_vector(self, frame, enemy_names) -> np.ndarray:
        return np.array([
            self._pm_indexer.get(enemy_names[i], playmaker[Layout.Playmaker.NAME],
                                 playmaker[Layout.Playmaker.STATE_NAME])
            for i, enemy in enumerate(frame[Layout.Frame.ENEMIES])
            for playmaker in enemy[Layout.Enemy.PLAY_MAKERS]
        ], dtype=np.int32)

    def _get_mean_of_cont(self) -> np.array: #TODO: exclude booleans

        num_enemies = 3 #TODO: have it saved somewhere per whole dataset
        num_cont = Layout.Hero.num_elements + num_enemies * (Layout.Enemy.num_elements -1)
        mean = np.zeros([num_cont], dtype=np.float32)
        n_frames = 0

        for frame, _ in self._reader.iterate_frames(False):
            n_frames += 1
            mean += self._build_continuous_vector(frame)

        return mean / n_frames

    def _get_std_of_cont(self)-> np.array: #TODO: exclude booleans

        num_enemies = 3 #TODO: have it saved somewhere per whole dataset
        num_cont = Layout.Hero.num_elements + num_enemies * (Layout.Enemy.num_elements -1)
        var = np.zeros([num_cont], dtype=np.float32)
        n_frames = 0
        for frame, _ in self._reader.iterate_frames(False):
            n_frames += 1
            var += np.pow(self._build_continuous_vector(frame) - self._mean, 2)

        std = np.sqrt(var / (n_frames-1)) # let's pray to god we never get only one frame
        std[std == 0] = 1.0
        return std

    def run(self, output: str):
        for frame, recording_info in self._reader.iterate_frames(self._require_success):

            cont = np.divide(self._build_continuous_vector(frame) - self._mean, self._std)
            fsm = self._build_playmaker_vector(frame, recording_info["EnemyNames"])

            inputs = np.array(frame[Layout.Frame.INPUTS], dtype=np.float32)

            if inputs[3] > 0.32 and inputs[3] < 0.34:
                print(np.divide(cont - self._mean, self._std))
                print(fsm)
                print(inputs)
                return


if __name__ == "__main__":
    reader = RawDatasetReader("../datasets/Mossbone Mother")
    preprocessor = Preprocessor(reader, True)
    preprocessor.run("")