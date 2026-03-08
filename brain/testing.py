from pprint import pprint

import torch

from data_processing.layout import UserInputs, NamedState
from data_processing.preprocessing import ProcessingPipeline, FrameProcessor
from data_processing.recording_reader import RecordingFilters, ChoosePercentBest, RecordingReader
from data_processing.vocabulary import Vocabulary


def test_layout():
    recording_filter = RecordingFilters()
    recording_filter.quality_filter = ChoosePercentBest(0.5)
    reader = RecordingReader("../recordings/Mossbone Mother", "Mossbone Mother", recording_filter)

    frame = next(reader.frames())[0]

    pprint(frame)
    print(f"Named states: {frame.FrameData.type_count([NamedState])}")
    print(f"Continuous: {frame.FrameData.continuous_count()}")
    print(f"Boolean: {frame.FrameData.boolean_count()}")

def test_vocab():

    recording_filter = RecordingFilters()
    recording_filter.quality_filter = ChoosePercentBest(0.5)
    reader = RecordingReader("../recordings/Mossbone Mother", "Mossbone Mother", recording_filter)

    vocab = Vocabulary()
    vocab.from_recordings(reader)

    print(vocab.dictionary)

def test_processing():
    recording_filter = RecordingFilters()
    recording_filter.quality_filter = ChoosePercentBest(0.5)

    pp = ProcessingPipeline("../recordings/Mossbone Mother", "Mossbone Mother", recording_filter)

    pp.process("data_processing/temp", 1)

    a = torch.load("data_processing/temp/training_data.pt", weights_only=False)
    print("\n\n\n\n\n\n\n\n")
    pprint(a["frame_processor"])

    procesor = FrameProcessor.from_dict(a["frame_processor"])

if __name__ == "__main__":
    #test_layout()
    #test_vocab()
    test_processing()
