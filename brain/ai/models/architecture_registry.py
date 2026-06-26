from typing import Type

from .base_model import BaseBossNet

model_registry: dict[str, Type[BaseBossNet]] = {}

def register_model(cls):
    if not issubclass(cls, BaseBossNet):
        raise Exception('Model class must be a subclass of BaseBossNet')

    model_registry[cls.__name__] = cls
    return cls