from typing import Type

from base_model import BaseBossNet

model_registry: dict[str, Type[BaseBossNet]] = {}

def register_model(cls):
    model_registry[cls.__name__] = cls
    return cls