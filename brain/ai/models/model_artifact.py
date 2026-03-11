import os
from datetime import datetime
from pathlib import Path
import torch
from .base_model import BaseBossNet
from .model_factory import ModelFactory
from data_processing import FrameProcessor, SUPPORTED_LAYOUT_VERSION
from shared import ModelDimensions

class Artifact:
    def __init__(
            self,
            model: BaseBossNet,
            frame_processor: FrameProcessor,
            boss_name: str,
            metadata: dict = None,
    ):
        self.model: BaseBossNet = model
        self.frame_processor = frame_processor
        self.boss_name = boss_name
        self.metadata = metadata if metadata is not None else Artifact.get_empty_metadata()

    def save(self, file):
        torch.save({
            "layout_version": SUPPORTED_LAYOUT_VERSION,
            "boss_name": self.boss_name,
            "model_state_dict": self.model.state_dict(),
            "model_config": self.model.get_dict_config(),
            "frame_processor": self.frame_processor.to_dict(),
            "metadata": self.metadata,
        }, file)

    @staticmethod
    def get_empty_metadata():
        return {
            "created": datetime.now().strftime("%Y-%m-%d %H:%M"),
            "behavioral_cloning": None, #TODO
            "reinforcement_learning": None, #TODO
        }


class ArtifactFactory:
    @staticmethod
    def from_file(model_path: Path) -> Artifact:

        if not os.path.exists(model_path) or os.path.isdir(model_path):
            raise Exception(f"File does not exist: {model_path}")

        data = torch.load(model_path, weights_only=False)
        if data["layout_version"] != SUPPORTED_LAYOUT_VERSION:
            raise Exception(f"Layout version mismatch (supported: {SUPPORTED_LAYOUT_VERSION}, in dataset: {data['layout_version']})")

        model_config = data["model_config"]
        model = ModelFactory.construct(**model_config)

        model.load_state_dict(data["model_state_dict"])

        frame_processor = FrameProcessor.from_dict(data["frame_processor"])

        return Artifact(model, frame_processor, data["boss_name"], data["metadata"])


    @staticmethod
    def from_dataset(dataset_path: Path, model_type: str, **kwargs) -> Artifact:
        """
        :param network_type:
        :param path:
        :param kwargs: additional arguments needed to construct the network. In practice the ModelDimensions are stored
        inside the dataset so only the additional arguments are needed.
        :return:
        """
        testing_path = os.path.join(dataset_path, "testing_data.pt")
        training_path = os.path.join(dataset_path, "training_data.pt")

        if (   not os.path.exists(dataset_path)
            or not os.path.exists(testing_path)
            or not os.path.exists(training_path)):

            raise Exception("Path does not exist or does not contain the dataset")

        data = torch.load(training_path, weights_only=False)
        if data["layout_version"] != SUPPORTED_LAYOUT_VERSION:
            raise Exception(f"Layout version mismatch (supported: {SUPPORTED_LAYOUT_VERSION}, in dataset: {data['layout_version']})")

        dims = ModelDimensions(**data["dims"])

        model = ModelFactory.construct(model_type, dims, **kwargs)
        frame_processor = FrameProcessor.from_dict(data["frame_processor"])

        return Artifact(model, frame_processor, data["target_boss"])