import os
import time
from dataclasses import fields
from pprint import pprint

import torch
from shared import config, load_config
from data_processing import UserInputs, NamedState, RecordingFilters, ChoosePercentBest, RecordingReader, Vocabulary, \
    RecordingProcessor

config = load_config()

def test_layout():
    recording_filter = RecordingFilters()
    recording_filter.quality_filter = ChoosePercentBest(0.5)

    recording_path = os.path.join(config.recording_dir, "Lace Boss1")
    reader = RecordingReader(recording_path, "Lace Boss1", recording_filter)


    start = time.perf_counter()
    for i in range(0, 50): #1.189s
        for frame, _ in reader.frames():
            a = 2
    end = time.perf_counter()


    # print(f"Executed in {end - start:0.4f} seconds")
    # print("\n\n\n\n\n")
    frame = next(reader.frames())[0]
    # start = time.perf_counter()
    # for i in range (0, 100000):
    #     a = frame.get_all_of_type_fast(bool)
    #     a1 = frame.get_all_of_type_fast(int)
    #     a2 = frame.get_all_of_type_fast(float)
    #     a3 = frame.get_all_of_type_fast(NamedState)
    # end = time.perf_counter()
    # print(f"Executed in {end - start:0.4f} seconds")

    start = time.perf_counter()
    for i in range (0, 100000):
        b = frame.get_all_of_type(bool)
        b1 = frame.get_all_of_type(int)
        b2 = frame.get_all_of_type(float)
        b3 = frame.get_all_of_type(NamedState)
    end = time.perf_counter()
    print(f"Executed in {end - start:0.4f} seconds")

    print(a)
    print(b)
    print(a1)
    print(b1)
    print(a2)
    print(b2)
    print(a3)
    print(b3)


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

    pp = RecordingProcessor("../recordings/Mossbone Mother", "Mossbone Mother", recording_filter)

    pp.process_and_save("temp/dataset", 1)

    # a = torch.load("data_processing/temp/training_data.pt", weights_only=False)
    # print("\n\n\n\n\n\n\n\n")
    # pprint(a["frame_processor"])
    #
    # procesor = FrameProcessor.from_dict(a["frame_processor"])

a = [[0, 1, 2, 3], [4, 5], [6, 7, 8]]


if __name__ == "__main__":
    test_layout()
    #test_vocab()
    # test_processing()
