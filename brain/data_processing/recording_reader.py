import math
import msgpack
from abc import ABC, abstractmethod
from dataclasses import dataclass
from os import listdir
from os.path import isfile, join
from typing import Optional, Iterable, Tuple, List

from .layout import SUPPORTED_LAYOUT_VERSION, RecordingFrame


@dataclass
class RecordingFile:
    filename: str
    info: dict

class RecordingQualityFilter(ABC):
    @staticmethod
    def _sort_by_reward(files: list[RecordingFile]) -> list[RecordingFile]:
        """
        sorts recordings from highest to lowest reward
        """
        return sorted(files, key=lambda x: x.info.get("total_reward", 0), reverse=True)
    @staticmethod
    def _sort_by_length(files: list[RecordingFile]) -> list[RecordingFile]:
        """
        sorts recordings from shortest to longest
        """
        return sorted(files, key=lambda x: x.info.get("frame_count", 0))
    @abstractmethod
    def apply(self, files: list[RecordingFile]) -> list[RecordingFile]:
        pass

class ChooseNBest(RecordingQualityFilter):
    def __init__(self, count: int) -> None:
        self.count = count
    def apply(self, files: list[RecordingFile]) -> list[RecordingFile]:
        return self._sort_by_reward(files)[:self.count]

class ChoosePercentBest(RecordingQualityFilter):
    def __init__(self, percent: float) -> None:
        """
        :param percent: value in range [0, 1]
        """
        self.percent = percent
    def apply(self, files: list[RecordingFile]) -> list[RecordingFile]:
        count = math.ceil(len(files) * self.percent)
        count = min(count, len(files))
        return self._sort_by_reward(files)[:count]

@dataclass
class RecordingFilters:
    require_success: bool = True
    record_frame_delta: int = 10
    player_name: str = None
    quality_filter: Optional[RecordingQualityFilter] = None

    def matches(self, info: dict, filename: str):
        matches = True
        if self.require_success and info['success'] == False:
            print(f"Recording {filename} does not match require_success filter")
            matches = False

        if self.record_frame_delta != info['record_frame_delta']:
            print(f"Recording {filename} does not match record_frame_delta filter")
            matches = False

        if self.player_name is not None and info['player_name'] != self.player_name:
            print(f"Recording {filename} does not match player_name filter")
            matches = False

        return matches


class RecordingReader:
    def __init__(self, data_path: str, target_boss: str, filters: Optional[RecordingFilters] = None):
        self._data_path = data_path
        self.target_boss = target_boss
        self.filters = filters

        self.filtered_recordings = self.get_filtered_recording_files()
        if len(self.filtered_recordings) == 0: raise Exception("No Usable Files Found!!!")

    def get_filtered_recording_files(self) -> list[RecordingFile]:
        filenames = [f for f in listdir(self._data_path) if isfile(join(self._data_path, f))]

        dataset_files: list[RecordingFile] = []
        # group the info and data files together, make sure we have both .info and .msgpack files:

        for idx, filename in enumerate(filenames):
            if not filename.endswith(".msgpack"): continue

            with (open(join(self._data_path, filename), "rb") as f):
                unpacker = msgpack.Unpacker(f, raw=False)
                try:
                    header = next(unpacker)
                except StopIteration:
                    continue

                format_version = header.get("format_version", 1)
                if format_version != SUPPORTED_LAYOUT_VERSION:
                    print(f"skipping recording {filename}, recording's format version: {format_version}")
                    continue

                if header.get("target_name") != self.target_boss:
                    continue

                footer = None
                for obj in unpacker:
                    footer = obj

                info = dict(header)
                info.update(footer)
                info.update({"recording_id": idx})

                if self.filters is not None and not self.filters.matches(info, filename):
                    continue

                dataset_files.append(RecordingFile(
                    filename=filename,
                    info=info,
                ))

        if self.filters is not None and self.filters.quality_filter is not None:
            dataset_files = self.filters.quality_filter.apply(dataset_files)

        return dataset_files

    def frames(self) -> Iterable[Tuple[RecordingFrame, dict]]:
        """
        :return: yields frames and recording information
        """
        for recording in self.filtered_recordings:
            with open(join(self._data_path, recording.filename), "rb") as f:
                unpacker = msgpack.Unpacker(f, raw=False)
                try:
                    header = next(unpacker)
                except StopIteration:
                    continue

                prev_raw_frame = None
                for raw_frame in unpacker:
                    if prev_raw_frame is not None:
                        yield RecordingFrame.from_indexed_list(prev_raw_frame), recording.info
                    prev_raw_frame = raw_frame

    def get_frame_count(self) -> int:
        """
        :return: Total number of frames
        """
        frame_count: int = 0
        for recording in self.filtered_recordings:
            frame_count += recording.info["frame_count"]

        return frame_count

    def get_frame_counts(self) -> List[int]:
        """
        :return: Number of frames per recording
        """
        frame_counts: List[int]= []
        for recording in self.filtered_recordings:
            frame_counts.append(recording.info["frame_count"])

        return frame_counts

    def get_recording_count(self) -> int:
        return len(self.filtered_recordings)