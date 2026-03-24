from ai import models
from networking.server import ClientServices
from networking.handler_registry import register_handler


@register_handler("get_architectures")
async def get_architectures(payload: dict, services: ClientServices):
    payload = {}
    for name, architecture_cls in models.model_registry.items():
        payload[name] = architecture_cls.serialize_params()
    return {"architectures": payload}