from abc import ABC, abstractmethod
from torch import nn

class BaseBossNet(nn.Module, ABC):
    """
    Every network should inherit from this class.
    """
    def __init__(self):
        super().__init__()

    @abstractmethod
    def get_dict_config(self) -> dict:
        """
        :return: parameters needed to reconstruct the network. Must contain the class name inside entry called "type"
        """
        pass
