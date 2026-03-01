import json
import os
import re
from logging import warning
from optparse import Option
from os import listdir
from os.path import isfile, join
from typing import Iterable, Tuple, Generator, Iterator, Optional
import msgpack
import numpy as np
import torch
from sympy.codegen import Print

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

        for id, filename in enumerate(filenames):
            if not filename.endswith(".msgpack"): continue

            with (open(join(self._data_path, filename), "rb") as f):
                unpacker = msgpack.Unpacker(f, raw=False)
                try:
                    header = next(unpacker)
                except StopIteration:
                    continue
                format_version = header.get("Format Version", int) #TODO remove space from "Format Version"

                if format_version != Layout.SUPPORTED_FORMAT_VERSION or header.get("BossName") != self.target_boss:
                    continue
                #TODO: add some checking whether in all recordings the number of enemies is constant!!!!
                footer = None
                for obj in unpacker:
                    footer = obj

                info = dict(header)
                info.update(footer)
                info.update({"RecordingID": id})

                dataset_files.append(DatasetFile(
                    filename=filename,
                    info=info,
                ))

        return dataset_files

    def get_enemy_count(self) -> int:
        return len(self.usable_files[0].info["EnemyNames"])

    def iterate_frames(self, require_success) -> Iterable[Tuple[list[any], dict]]:
        """
        :param require_success:
        :return: yields next frame and recording information
        """
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

    def get_total_frame_count(self, require_success) -> int:
        frame_count: int = 0
        for dataset in self.usable_files:
            if require_success and dataset.info["Success"] != True: continue

            frame_count += dataset.info["FrameCount"]

        return frame_count


class Vocabulary: #TODO: add _dictionary saving #TODO: make this also reusable for live preprocessing
    def __init__(self):

        self._dictionary: dict[str, int] = dict()
        self._indexed = False

        self._instance_regex = re.compile(r' \(\d+\)$')

    def create_from_recordings(self, reader: RawDatasetReader):
        # Very important - this cuts out the instance number ((1),(2) etc.) when later doing anything with this data this has to be taken into account

        vocab: set[str] = set()
        for enemy_name, playmaker_name, state_name in self._get_playmaker_pairs_per_enemy(reader):
            vocab.add(f"{re.sub(self._instance_regex, '', enemy_name)}|{playmaker_name}|{state_name}")

        for i, word in enumerate(sorted(vocab)):
            self._dictionary[word] = i

        self._indexed = True

    @staticmethod
    def _get_playmaker_pairs_per_enemy(reader: RawDatasetReader) -> Iterable[tuple[str, str, str]]:
        for frame, recording_info in reader.iterate_frames(require_success=False):
            enemy_list = frame[Layout.Frame.ENEMIES]
            if not enemy_list:
                continue

            for enemy_data, enemy_name in zip(enemy_list, recording_info["EnemyNames"]):
                if enemy_data is None:
                    continue

                pms = (enemy_data[Layout.Enemy.PLAYMAKERS] or [["", ""]])  # no playmakers -> add placeholder token #TODO rethink in foreseeable future
                for playmaker in pms:
                    yield enemy_name, playmaker[Layout.Playmaker.NAME], playmaker[Layout.Playmaker.STATE_NAME]

    def get(self, enemy_name: str, playmaker_name: str, state_name:str) -> int:
        if not self._indexed:
            raise Exception("Not indexed! You have to call run() first!")

        word = f"{re.sub(self._instance_regex, '', enemy_name)}|{playmaker_name}|{state_name}"
        output = self._dictionary[word]

        if output is None:
            raise Exception(f"Word not found in vocabulary!: {word}")

        return output

    def __len__(self):
        return self.get_word_count()

    def get_word_count(self):
        return len(self._dictionary)

    def load(self, file:str) -> None:
        with open(file, 'r') as fp:
            self._dictionary = json.load(fp)

    def save(self, file:str) -> None:
        with open(file, 'w') as fp:
            json.dump(self._dictionary, fp, indent=2)



class Preprocessor:
    def __init__(self, reader: RawDatasetReader, vocabulary: Vocabulary):
        self._reader = reader
        self._vocabulary = vocabulary
        self._num_continuous = self.get_continuous_dim()

    def get_continuous_dim(self) -> int:
        return (
                Layout.Hero.num_elements +
                self._reader.get_enemy_count() *
                (Layout.Enemy.num_elements - 1)
        )

    def get_playmaker_dim(self) -> int:
        # Unfortunately the easiest way to get this is by reading a single line from the recording.
        # The number of playmakers throughout the recording/s for the same boss SHOULD be the same.
        # At least for bosses tested so far - enemies do not disappear from the scene once dead,
        # They are just hidden, also all the enemies that can appear during fight are spawned at the
        # same time as the boss. But this requires further looking into once more bosses are tested
        # another way we could determine this value would be to precompute it in recording and store inside
        # header - how many playmakers each enemy instance has. But its not that important for now
        frame = next(self._reader.iterate_frames(False))[0]
        playmaker_count = 0
        for enemy in frame[Layout.Frame.ENEMIES]:
            playmaker_count += len(enemy[Layout.Enemy.PLAYMAKERS])
        return playmaker_count

    @staticmethod
    def _build_continuous_vector(frame) -> np.ndarray:
        return np.array([
            *[val for val in frame[Layout.Frame.HERO]],
            *[val for enemy in frame[Layout.Frame.ENEMIES]
              for val in enemy[:Layout.Enemy.PLAYMAKERS]]
        ], dtype=np.float32)

    def _build_boolean_mask(self) -> np.ndarray:
        num_enemies = self._reader.get_enemy_count()
        return np.array([
            *[val for val in Layout.Hero.boolean_mask],
            *[val for _ in range(num_enemies)
              for val in Layout.Enemy.boolean_mask[:Layout.Enemy.PLAYMAKERS]]
        ], dtype=np.float32)

    def _get_mean_of_continuous(self) -> np.array:
        mean = np.zeros([self._num_continuous], dtype=np.float32)
        n_frames = 0

        for frame, _ in self._reader.iterate_frames(False):
            n_frames += 1
            mean += self._build_continuous_vector(frame)

        return mean / n_frames

    def _get_std_of_continuous(self, mean: np.ndarray)-> np.ndarray:
        var = np.zeros([self._num_continuous], dtype=np.float32)
        n_frames = 0
        for frame, _ in self._reader.iterate_frames(False):
            n_frames += 1
            var += np.pow(self._build_continuous_vector(frame) - mean, 2)

        std = np.sqrt(var / (n_frames-1)) # let's pray to god we never get only one frame
        std[std == 0] = 1.0
        return std

    def _build_playmaker_vector(self, frame, enemy_names) -> np.ndarray:
        return np.array([
            self._vocabulary.get(enemy_names[i], playmaker[Layout.Playmaker.NAME],
                                playmaker[Layout.Playmaker.STATE_NAME])
            for i, enemy in enumerate(frame[Layout.Frame.ENEMIES])
            for playmaker in enemy[Layout.Enemy.PLAYMAKERS]
        ], dtype=np.int32)

    def frame_data_generator(self, require_success: bool) -> Iterator[Tuple[np.ndarray, np.ndarray, np.ndarray, dict]]:
        """
        transforms the frames from raw recordings to format usable by neural network and yields each frame separately
        :return: continuous vector, playmaker id's vector, user_inputs vector, recording info dict
        """
        bm = self._build_boolean_mask()

        mean = self._get_mean_of_continuous()
        np.putmask(mean, bm, 0)

        std = self._get_std_of_continuous(mean)
        np.putmask(std, bm, 1)

        for frame, recording_info in self._reader.iterate_frames(require_success):

            continuous = self._build_continuous_vector(frame)
            continuous -= mean
            continuous /= std

            playmakers = self._build_playmaker_vector(frame, recording_info["EnemyNames"])
            user_inputs = np.array(frame[Layout.Frame.INPUTS], dtype=np.float32)

            yield continuous, playmakers, user_inputs, recording_info

class ProcessingPipeline:
    def __init__(self, data_path: str, target_boss: str, vocabulary: Optional[Vocabulary]=None):

        self._vocabulary = vocabulary
        self._data_reader = RawDatasetReader(data_path, target_boss)

        if self._vocabulary is None:
            print("ProcessingPipeline: No vocabulary provided. Creating a new one from recordings.")
            self._vocabulary = Vocabulary()
            self._vocabulary.create_from_recordings(self._data_reader)

        self._preprocessor = Preprocessor(self._data_reader, self._vocabulary)

    def verify_dataset_shape(self):
        expected_cont_dim = None
        expected_playmaker_dim = None

        for frame, recording_info in self._data_reader.iterate_frames(False):
            cont_len = Layout.Hero.num_elements + len(frame[Layout.Frame.ENEMIES]) * (Layout.Enemy.num_elements - 1)
            pm_len = sum(len(enemy[Layout.Enemy.PLAYMAKERS]) for enemy in frame[Layout.Frame.ENEMIES])

            if expected_cont_dim is None:
                expected_cont_dim = cont_len
            elif cont_len != expected_cont_dim:
                raise Exception(f"Continuous vector length mismatch! Got {cont_len}, expected {expected_cont_dim}")

            if expected_playmaker_dim is None:
                expected_playmaker_dim = pm_len
            elif pm_len != expected_playmaker_dim:
                raise Exception(f"Playmaker vector length mismatch! Got {pm_len}, expected {expected_playmaker_dim}")

        print(f"Dataset verification passed. Continuous={expected_cont_dim}, Playmakers={expected_playmaker_dim}")

    def process_and_save(self, dataset_name: str, output_dir: str, num_testing: int = 0, save_vocab: bool = False, verify_data_shape = True) -> None:
        """
        :param dataset_name: The name of the dataset used for saving.
        :param output_dir: directory of the output files
        :param num_testing: how many recordings should be saved separately for testing
        :param save_vocab: should the vocabulary be saved?
        :param verify_data_shape: should the dataset dimensions be verified?
        :return:
        """
        if verify_data_shape:
            self.verify_dataset_shape()

        cont_list = []
        playmaker_list = []
        target_list = []
        recording_frames_list = []


        prev_id = -1
        for continuous, playmaker, user_inputs, recording_info in self._preprocessor.frame_data_generator(True):

            current_id = recording_info.get("RecordingID")
            if current_id != prev_id:
                recording_frames_list.append(recording_info.get("FrameCount"))
                prev_id = current_id

            cont_list.append(continuous)
            playmaker_list.append(playmaker)
            target_list.append(user_inputs)

        if save_vocab:
            self._vocabulary.save(os.path.join(output_dir, f"{dataset_name}_vocabulary.json"))

        num_testing = min(num_testing, len(recording_frames_list))
        test_frames = sum(recording_frames_list[:num_testing])

        if num_testing > 0:
            data_test = {
                "cont_dim": self._preprocessor.get_continuous_dim(),
                "playmaker_dim": self._preprocessor.get_playmaker_dim(),
                "vocab_dim": self._vocabulary.get_word_count(),
                "frame_counts": recording_frames_list[:num_testing],
                "continuous": torch.tensor(np.stack(cont_list[:test_frames])),
                "playmakers": torch.tensor(np.stack(playmaker_list[:test_frames])),
                "targets": torch.tensor(np.stack(target_list[:test_frames])),
            }
            torch.save(data_test, os.path.join(output_dir, f"{dataset_name}_testing.pt"))

        data_train = {
            "cont_dim": self._preprocessor.get_continuous_dim(),
            "playmaker_dim": self._preprocessor.get_playmaker_dim(),
            "vocab_dim": self._vocabulary.get_word_count(),
            "frame_counts": recording_frames_list[num_testing:],
            "continuous": torch.tensor(np.stack(cont_list[test_frames:])),
            "playmakers": torch.tensor(np.stack(playmaker_list[test_frames:])),
            "targets": torch.tensor(np.stack(target_list[test_frames:])),
        }
        torch.save(data_train, os.path.join(output_dir, f"{dataset_name}_training.pt"))



if __name__ == "__main__":
    pp = ProcessingPipeline(data_path="../recordings/Mossbone Mother",
                       target_boss="Mossbone Mother")

    pp.process_and_save("Mossbone_Mother_Tests", output_dir="processed", save_vocab=True, num_testing=2)