from ai.models import get_available_artifacts
from networking.server import ClientServices
from networking.handler_registry import register_handler


@register_handler("get_artifacts")
async def get_models(payload: dict, services: ClientServices):
    output = {
        "artifacts": get_available_artifacts()
    }
    return output