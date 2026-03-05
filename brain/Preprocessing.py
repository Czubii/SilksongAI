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

from Networks import BossModelArtifact
from RecordingLayout import *


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
                format_version = header.get("format_version", 1)

                if format_version != Layout.SUPPORTED_FORMAT_VERSION or header.get("target_name") != self.target_boss:
                    print(f"skipping recording {filename}, recording's format version: {format_version}")
                    continue

                footer = None
                for obj in unpacker:
                    footer = obj

                info = dict(header)
                info.update(footer)
                info.update({"recording_id": id})

                dataset_files.append(DatasetFile(
                    filename=filename,
                    info=info,
                ))

        return dataset_files

    def get_enemy_count(self) -> int:
        return len(self.usable_files[0].info["enemy_names"])

    def iterate_frames(self, require_success) -> Iterable[Tuple[list[any], dict]]:
        """
        :param require_success:
        :return: yields next frame and recording information
        """
        for data_file in self.usable_files:
            if require_success and data_file.info["success"] != True: continue

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
            if require_success and dataset.info["success"] != True: continue

            frame_count += dataset.info["frame_count"]

        return frame_count


class Vocabulary:
    def __init__(self, raw_dictionary: dict[str, int] = None):

        self._dictionary: dict[str, int] = dict()
        self._indexed = False

        if raw_dictionary is not None:
            self._dictionary = raw_dictionary
            self._indexed = True

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
            enemy_list = frame[Layout.RecordingFrame.ENEMIES]
            if not enemy_list:
                continue

            for enemy_data, enemy_name in zip(enemy_list, recording_info["enemy_names"]):
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

    def get_raw(self):
        return self._dictionary


def build_playmaker_vector(vocabulary, frame, enemy_names) -> np.ndarray:
    return np.array([
        vocabulary.get(enemy_names[i], playmaker[Layout.Playmaker.NAME],
                       playmaker[Layout.Playmaker.STATE_NAME])
        for i, enemy in enumerate(frame[Layout.RecordingFrame.ENEMIES])
        for playmaker in enemy[Layout.Enemy.PLAYMAKERS]
    ], dtype=np.int32)

def build_continuous_vector(frame) -> np.ndarray:
    return np.array([
        *[val for val in frame[Layout.RecordingFrame.HERO]],
        *[val for enemy in frame[Layout.RecordingFrame.ENEMIES]
            for val in enemy[:Layout.Enemy.NAME]]
    ], dtype=np.float32)

def build_enemy_names_list(frame) -> List[str]:
    return [
        enemy[Layout.Enemy.NAME] for enemy in frame[Layout.RecordingFrame.ENEMIES]
    ]

class Preprocessor:
    def __init__(self, reader: RawDatasetReader, vocabulary: Vocabulary):
        self._reader = reader
        self._vocabulary = vocabulary
        self._num_continuous = self.get_continuous_dim()

        self.mean = self._get_mean_of_continuous()
        self.std = self._get_std_of_continuous(self.mean)

        bm = self._build_boolean_mask()

        np.putmask(self.mean, bm, 0)
        np.putmask(self.std, bm, 1)

    def get_continuous_dim(self) -> int:
        return (
                Layout.Hero.num_elements +
                self._reader.get_enemy_count() *
                (Layout.Enemy.num_elements - 2)
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
        for enemy in frame[Layout.RecordingFrame.ENEMIES]:
            playmaker_count += len(enemy[Layout.Enemy.PLAYMAKERS])
        return playmaker_count

    def _build_boolean_mask(self) -> np.ndarray:
        num_enemies = self._reader.get_enemy_count()
        return np.array([
            *[val for val in Layout.Hero.boolean_mask],
            *[val for _ in range(num_enemies)
              for val in Layout.Enemy.boolean_mask[:Layout.Enemy.NAME]]
        ], dtype=np.float32)

    def _get_mean_of_continuous(self) -> np.array:
        mean = np.zeros([self._num_continuous], dtype=np.float32)
        n_frames = 0

        for frame, _ in self._reader.iterate_frames(False):
            n_frames += 1
            mean += build_continuous_vector(frame)

        return mean / n_frames

    def _get_std_of_continuous(self, mean: np.ndarray)-> np.ndarray:
        var = np.zeros([self._num_continuous], dtype=np.float32)
        n_frames = 0
        for frame, _ in self._reader.iterate_frames(False):
            n_frames += 1
            var += np.pow(build_continuous_vector(frame) - mean, 2)

        std = np.sqrt(var / (n_frames-1)) # let's pray to god we never get only one frame
        std[std == 0] = 1.0
        return std

    def frame_data_generator(self, require_success: bool) -> Iterator[Tuple[np.ndarray, np.ndarray, np.ndarray, dict]]:
        """
        transforms the frames from raw recordings to format usable by neural network and yields each frame separately
        :return: continuous vector, playmaker id's vector, user_inputs vector, recording info dict
        """

        for frame, recording_info in self._reader.iterate_frames(require_success):

            continuous = build_continuous_vector(frame)
            continuous -= self.mean
            continuous /= self.std

            enemy_names = build_enemy_names_list(frame)

            playmakers = build_playmaker_vector(self._vocabulary, frame, enemy_names)
            user_inputs = np.array(frame[Layout.RecordingFrame.INPUTS], dtype=np.float32)

            yield continuous, playmakers, user_inputs, recording_info

class LivePreprocessor:
    """
    Used for live inference
    """
    def __init__(self, artifact: BossModelArtifact):
        self._vocabulary = Vocabulary(artifact.vocab)
        self._mean = artifact.mean
        self._std = artifact.std

    def process_frame(self, frame: List) -> Tuple[np.ndarray, np.ndarray]:
        try:
            continuous = build_continuous_vector(frame)
        except Exception as e:
            raise RuntimeError(
                f"[process_frame] Failed in build_continuous_vector | "
                f"frame_type={type(frame)} frame_len={len(frame) if frame is not None else 'None'}"
            ) from e

        try:
            continuous -= self._mean
        except Exception as e:
            raise RuntimeError(
                f"[process_frame] Failed during normalization subtraction | "
                f"continuous_shape={getattr(continuous, 'shape', None)} "
                f"mean_shape={getattr(self._mean, 'shape', None)}"
            ) from e

        try:
            continuous /= self._std
        except Exception as e:
            raise RuntimeError(
                f"[process_frame] Failed during normalization division | "
                f"continuous_shape={getattr(continuous, 'shape', None)} "
                f"std_shape={getattr(self._std, 'shape', None)}"
            ) from e

        try:
            enemy_names = build_enemy_names_list(frame)
        except Exception as e:
            raise RuntimeError(
                f"[process_frame] Failed in build_enemy_names_list | "
                f"frame_len={len(frame) if frame is not None else 'None'}"
            ) from e

        try:
            playmakers = build_playmaker_vector(self._vocabulary, frame, enemy_names)
        except Exception as e:
            raise RuntimeError(
                f"[process_frame] Failed in build_playmaker_vector | "
                f"vocab_size={len(self._vocabulary) if self._vocabulary is not None else 'None'} "
                f"enemy_names_count={len(enemy_names) if enemy_names is not None else 'None'}"
            ) from e

        return continuous, playmakers

class ProcessingPipeline:
    def __init__(self, data_path: str, target_boss: str, vocabulary: Optional[Vocabulary]=None):

        self._vocabulary = vocabulary
        self._data_reader = RawDatasetReader(data_path, target_boss)
        self._target_boss = target_boss

        if self._vocabulary is None:
            print("ProcessingPipeline: No vocabulary provided. Creating a new one from recordings.")
            self._vocabulary = Vocabulary()
            self._vocabulary.create_from_recordings(self._data_reader)

        self._preprocessor = Preprocessor(self._data_reader, self._vocabulary)

    def verify_dataset_shape(self):
        expected_cont_dim = None
        expected_playmaker_dim = None

        for frame, recording_info in self._data_reader.iterate_frames(False):
            cont_len = Layout.Hero.num_elements + len(frame[Layout.RecordingFrame.ENEMIES]) * (Layout.Enemy.num_elements - 1)
            pm_len = sum(len(enemy[Layout.Enemy.PLAYMAKERS]) for enemy in frame[Layout.RecordingFrame.ENEMIES])

            if expected_cont_dim is None:
                expected_cont_dim = cont_len
            elif cont_len != expected_cont_dim:
                raise Exception(f"ProcessingPipeline: Continuous vector length mismatch! Got {cont_len}, expected {expected_cont_dim}")

            if expected_playmaker_dim is None:
                expected_playmaker_dim = pm_len
            elif pm_len != expected_playmaker_dim:
                raise Exception(f"ProcessingPipeline: Playmaker vector length mismatch! Got {pm_len}, expected {expected_playmaker_dim}")

        print(f"ProcessingPipeline: Recording verification passed")

    def process_and_save(self, output_dir: str, num_testing: int = 0, verify_data_shape = True) -> None:
        """
        :param output_dir: directory of the output files
        :param num_testing: how many recordings should be saved separately for testing
        :param verify_data_shape: should the dataset dimensions be verified?
        :return:
        """
        if verify_data_shape:
            try:
                self.verify_dataset_shape()
            except Exception as e:
                raise RuntimeError(e)

        cont_list = []
        playmaker_list = []
        target_list = []
        recording_frames_list = []


        prev_id = -1
        for continuous, playmaker, user_inputs, recording_info in self._preprocessor.frame_data_generator(True):

            current_id = recording_info.get("recording_id")
            if current_id != prev_id:
                recording_frames_list.append(recording_info.get("frame_count"))
                prev_id = current_id

            cont_list.append(continuous)
            playmaker_list.append(playmaker)
            target_list.append(user_inputs)

        num_testing = min(num_testing, len(recording_frames_list))
        test_frames = sum(recording_frames_list[:num_testing])

        if num_testing > 0:
            data_test = {
                "layout_version": Layout.SUPPORTED_FORMAT_VERSION,
                "target_boss": self._target_boss,
                "frame_counts": recording_frames_list[:num_testing],
                "continuous": torch.tensor(np.stack(cont_list[:test_frames])),
                "playmakers": torch.tensor(np.stack(playmaker_list[:test_frames])),
                "targets": torch.tensor(np.stack(target_list[:test_frames])),
            }
            torch.save(data_test, os.path.join(output_dir, f"testing_data.pt"))

        data_train = {
            "layout_version": Layout.SUPPORTED_FORMAT_VERSION,
            "target_boss": self._target_boss,
            "frame_counts": recording_frames_list[num_testing:],
            "continuous": torch.tensor(np.stack(cont_list[test_frames:])),
            "playmakers": torch.tensor(np.stack(playmaker_list[test_frames:])),
            "targets": torch.tensor(np.stack(target_list[test_frames:])),
        }
        torch.save(data_train, os.path.join(output_dir, f"training_data.pt"))

        print(f"dataset saved in {output_dir}")

    def get_vocabulary(self):
        return self._vocabulary

    def get_dims(self) -> dict[str, int]:
        return {
            "cont_dim": self._preprocessor.get_continuous_dim(),
            "playmaker_dim": self._preprocessor.get_playmaker_dim(),
            "vocab_dim": self._vocabulary.get_word_count()
        }

    def get_cont_mean(self):
        return self._preprocessor.mean

    def get_cont_std(self):
        return self._preprocessor.std
