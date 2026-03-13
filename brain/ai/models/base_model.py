import inspect
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
        """
        :param base_dim: base dimensions inferred from dataset
        :param kwargs: parameters needed by specific architecture. (You need to pass every parameter used in constructor in
        order to allow the artifact to get serialized and reconstructed. Also, the keyword names must exactly match the names used in
        constructor)
        """

        self._additional_params = kwargs

        self.time_window = 1 # reference to this one is needed in order to for example initialize the dataset from base class
        self.base_dimensions = base_dim
        super().__init__()

    def get_dict_config(self) -> dict:
        """
        :return: dictionary containing blueprint to recreate the model inside model_factory
        """
        return {
            "model_type": self.__class__.__name__,
            "base_dims": self.base_dimensions,
            **self._additional_params
        }

    @abstractmethod
    def forward(self,
                cont_tensor: torch.Tensor,
                bool_tensor: torch.Tensor,
                named_state_tensor: torch.Tensor) -> Tuple[torch.Tensor, torch.Tensor]:
        pass

    @classmethod
    def get_additional_param_definitions(cls):
        """
        :return: Parameters that cannot be inferred from dataset inside the factory and must be explicitly passed by user.
        Used by the plugin to allow for creation of new networks by client.
        Output dictionary keys:
        "name" - name of parameter
        "type" - type of parameter
        "value" - here the default value of parameter
        """
        sig = inspect.signature(cls.__init__)
        additional_params = []

        for name, param in sig.parameters.items():
            if name in ("self", "base_dimensions"):
                continue

            default = str(param.default)
            if param.default == inspect.Parameter.empty:
                default = ""

            additional_params.append(
                {"name": name,
                 "type": param.annotation.__name__,
                 "value": default})

        return additional_params

