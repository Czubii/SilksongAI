import torch
from data_processing.layout import SUPPORTED_LAYOUT_VERSION
from data_processing.preprocessing import FrameProcessor
from ai.base_model import BaseBossNet
from ai.registry import model_registry

class BossNetArtifact:
    def __init__(
            self,
            model: BaseBossNet,
            frame_processor: FrameProcessor,
            boss_name: str,
            metadata: dict,
    ):
        self.model: BaseBossNet = model
        self.frame_processor = frame_processor
        self.boss_name = boss_name
        self.metadata = metadata

    def save(self, path):
        torch.save({
            "format_version": SUPPORTED_LAYOUT_VERSION,
            "state_dict": self.model.state_dict(),
            "config": self.model.get_dict_config(),
            "frame_processor": self.frame_processor.to_dict(),
            "boss_name": self.boss_name,
            "metadata": self.metadata,
        }, path)


class BossNetArtifactFactory:
    @staticmethod
    def from_file(path) -> BossNetArtifact:
        data = torch.load(path)

        model_type = data["type"]

        if not model_registry[model_type]:
            raise Exception("Unknown model type")

        model = model_registry[model_type](**data["config"])

    @staticmethod
    def from_dataset(dataset_path: str, model_type: str, **kwargs):
        """
        :param network_type:
        :param path:
        :param kwargs: additional arguments needed to construct the network. In practice the ModelDimensions are stored
        inside the dataset so only the additional arguments are needed.
        :return:
        """

        if not model_type in model_registry.keys():
            raise Exception("Unknown model type")


if __name__ == "__main__":
    print(model_registry)
    a = BossNetArtifactFactory.from_dataset("", "BossNet")