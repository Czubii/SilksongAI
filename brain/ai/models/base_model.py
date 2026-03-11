from abc import ABC, abstractmethod
from typing import Optional, Tuple
import torch
from torch import nn
from shared import ModelDimensions


class BaseBossNet(nn.Module, ABC):
    """
    Every network should inherit from this class.
    """
    def __init__(self, base_dim: Optional[ModelDimensions] = None, **kwargs):
        self.time_window = 1
        self.base_dimensions = base_dim
        super().__init__()

    @abstractmethod
    def get_dict_config(self) -> dict:
        """
        :return: parameters needed to reconstruct the network. Must contain the class name inside key called "type"
        """
        pass

    @abstractmethod
    def forward(self,
                cont_tensor: torch.Tensor,
                bool_tensor: torch.Tensor,
                named_state_tensor: torch.Tensor) -> Tuple[torch.Tensor, torch.Tensor]:
        pass

