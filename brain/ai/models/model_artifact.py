import os
from datetime import datetime
from pathlib import Path
from typing import List

import numpy as np
import torch

from data_processing import FrameProcessor, SUPPORTED_LAYOUT_VERSION
from shared import ModelDimensions
from .base_model import BaseBossNet
from .model_factory import ModelFactory


class Artifact:
    def __init__(
            self,
            model: BaseBossNet,
            frame_processor: FrameProcessor,
            boolean_thresholds: List[float],
            name: str,
            boss_name: str,
            metadata: dict = None,
    ):
        self.model: BaseBossNet = model
        self.frame_processor = frame_processor
        self.boolean_thresholds = boolean_thresholds
        self.boss_name = boss_name
        self.name = name
        self.metadata = metadata if metadata is not None else Artifact.get_empty_metadata()

    def save(self, file):

        torch.save({
            "layout_version": SUPPORTED_LAYOUT_VERSION,
            "boss_name": self.boss_name,
            "name": self.name,
            "model_state_dict": self.model.state_dict(),
            "model_config": self.model.get_dict_config(),
            "frame_processor": self.frame_processor.to_dict(),
            "boolean_thresholds": self.boolean_thresholds,
            "metadata": self.metadata,
        }, file)

    @staticmethod
    def get_empty_metadata():
        return {
            "created": datetime.now().strftime("%Y-%m-%d %H:%M"),
            "behavioral_cloning": {
                "total_epochs": 0,
                "losses": [],
            },
            "reinforcement_learning": None, #TODO
        }


class ArtifactFactory:
    @staticmethod
    def from_file(model_path: Path) -> Artifact:

        if not os.path.exists(model_path) or os.path.isdir(model_path):
            raise Exception(f"File does not exist: {model_path}")

        data = torch.load(model_path, weights_only=False)
        if data["layout_version"] != SUPPORTED_LAYOUT_VERSION:
            raise Exception(f"Layout version mismatch (supported: {SUPPORTED_LAYOUT_VERSION}, "
                            f"in artifact: {data['layout_version']})")

        model_config = data["model_config"]
        model = ModelFactory.construct(**model_config)

        model.load_state_dict(data["model_state_dict"])

        frame_processor = FrameProcessor.from_dict(data["frame_processor"])

        return Artifact(model, frame_processor, data["boolean_thresholds"], data["name"], data["boss_name"], data["metadata"])


    @staticmethod
    def new_from_dataset(training_dataset: Path,
                         testing_dataset: Path,
                         boss_name: str,
                         artifact_name:str,
                         model_type: str,
                         **kwargs) -> Artifact:
        """
        :param kwargs: additional arguments needed to construct the network. In practice the ModelDimensions are stored
        inside the dataset so only the additional arguments are needed.
        :return:
        """

        if (   not os.path.exists(testing_dataset)
            or not os.path.exists(training_dataset)):

            raise Exception("Path does not exist or does not contain the dataset")

        data = torch.load(training_dataset, weights_only=False)
        if data["layout_version"] != SUPPORTED_LAYOUT_VERSION:
            raise Exception(f"Layout version mismatch (supported: {SUPPORTED_LAYOUT_VERSION}, in dataset: {data['layout_version']})")

        if data["target_boss"] != boss_name:
            raise Exception(f"dataset contains different than expected target boss "
                            f"(dataset boss: {data["target_boss"]}, expected: {boss_name})")

        dims = ModelDimensions(**data["dims"])

        model = ModelFactory.construct(model_type, dims, **kwargs)
        frame_processor = FrameProcessor.from_dict(data["frame_processor"])

        boolean_thresholds = 0.5 * np.ones([dims.output_boolean])

        return Artifact(model, frame_processor, boolean_thresholds, artifact_name, boss_name)