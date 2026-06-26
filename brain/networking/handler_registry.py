from dataclasses import dataclass
from typing import Callable

@dataclass
class Handle:
    host_only: bool
    func: Callable

handler_registry: dict[str, Handle] = {}

def register_handler(name, host_only: bool):
    def decorator(func):
        handler_registry[name] = Handle(host_only, func)
        return func
    return decorator