from typing import Iterable
import re

from .layout import NamedStatesContainer
from .recording_reader import RecordingReader


class Vocabulary:
    def __init__(self, dictionary: dict[str, int] = None):
        self.dictionary: dict[str, int] = dictionary if dictionary is not None else {}
        self._instance_regex = re.compile(r' \(\d+\)$')

    def from_recordings(self, reader: RecordingReader) -> None:
        '''
        Very important - this cuts out the instance number ((1),(2) etc.) from parent name. When later doing anything
        with this data this has to be taken into account
        '''

        vocab: set[str] = set()
        for parent_name, name, state_name in self._named_state_generator(reader):
            vocab.add(f"{re.sub(self._instance_regex, '', parent_name)}|{name}|{state_name}")

        for i, word in enumerate(sorted(vocab)):
            self.dictionary[word] = i

    @staticmethod
    def _named_state_generator(reader: RecordingReader) -> Iterable[tuple[str, str, str]]:
        for frame, _ in reader.frames():
            named_state_containers = frame.FrameData.get_all_of_type([NamedStatesContainer])

            for container in named_state_containers:
                for named_state in container.NamedStates:
                    yield container.ParentName, named_state.Name, named_state.StateName

    def get(self, parent_name: str, name: str, state_name: str) -> int:
        if self.dictionary is None:
            raise Exception("Dictionary is empty")

        word = f"{re.sub(self._instance_regex, '', parent_name)}|{name}|{state_name}"
        output = self.dictionary[word]

        if output is None:
            raise Exception(f"Word not found in vocabulary!: {word}")

        return output

    def __len__(self):
        return len(self.dictionary)

    def to_dict(self):
        return self.dictionary

    @classmethod
    def from_dict(cls, dictionary: dict[str, int]):
        return cls(dictionary)