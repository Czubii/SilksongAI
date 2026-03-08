from abc import ABC, abstractmethod
from dataclasses import dataclass
from torch import nn
from model_dimensions import ModelDimensions


class BaseBossNet(nn.Module, ABC):
    """
    Every network should inherit from this class.
    """
    def __init__(self, base_dimensions: ModelDimensions):
        super().__init__()
