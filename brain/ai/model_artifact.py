import os
from datetime import datetime
from pprint import pprint
import torch
from data_processing.layout import SUPPORTED_LAYOUT_VERSION
from data_processing.preprocessing import FrameProcessor
from ai.models import BaseBossNet, model_registry, ModelFactory
from model_dimensions import ModelDimensions

class BossNetArtifact:
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
        self.metadata = metadata if metadata is not None else BossNetArtifact.get_empty_metadata()

    def save(self, dir, name, overwrite=False):

        output_path = os.path.join(dir, name+".pt")

        if not overwrite and os.path.exists(output_path):
            counter = 1
            while os.path.exists(output_path):
                output_path = os.path.join(dir, name + "(" + str(counter) + ").pt")
                counter += 1

        torch.save({
            "layout_version": SUPPORTED_LAYOUT_VERSION,
            "boss_name": self.boss_name,
            "model_state_dict": self.model.state_dict(),
            "model_config": self.model.get_dict_config(),
            "frame_processor": self.frame_processor.to_dict(),
            "metadata": self.metadata,
        }, output_path)

    @staticmethod
    def get_empty_metadata():
        return {
            "created": datetime.now().strftime("%Y-%m-%d %H:%M"),
            "behavioral_cloning": None, #TODO
            "reinforcement_learning": None, #TODO
        }


class BossNetArtifactFactory:
    @staticmethod
    def from_file(model_path) -> BossNetArtifact:

        if not os.path.exists(model_path) or os.path.isdir(model_path):
            raise Exception(f"File does not exist: {model_path}")

        data = torch.load(model_path, weights_only=False)
        if data["layout_version"] != SUPPORTED_LAYOUT_VERSION:
            raise Exception(f"Layout version mismatch (supported: {SUPPORTED_LAYOUT_VERSION}, in dataset: {data['layout_version']})")

        model_config = data["model_config"]
        model = ModelFactory.construct(**model_config)

        frame_processor = FrameProcessor.from_dict(data["frame_processor"])

        return BossNetArtifact(model, frame_processor, data["boss_name"], data["metadata"])


    @staticmethod
    def from_dataset(dataset_path: str, model_type: str, **kwargs) -> BossNetArtifact:
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

        return BossNetArtifact(model, frame_processor, data["target_boss"])


if __name__ == "__main__":
    a = BossNetArtifactFactory.from_dataset("../temp/dataset", "BossNet",
                                            time_window = 5,
                                            embedding_dim = 3,
                                            hidden_dim = 8)

    a.save("../temp/", "model", True)
    print(a)

    b = BossNetArtifactFactory.from_file("../temp/model.pt")