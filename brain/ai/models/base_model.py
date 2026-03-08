from abc import ABC, abstractmethod
from typing import Optional

from torch import nn
from model_dimensions import ModelDimensions


class BaseBossNet(nn.Module, ABC):
    """
    Every network should inherit from this class.
    """
    def __init__(self, base_dim: Optional[ModelDimensions] = None, **kwargs):
        super().__init__()

    @abstractmethod
    def get_dict_config(self) -> dict:
        """
        :return: parameters needed to reconstruct the network. Must contain the class name inside key called "type"
        """
        pass
