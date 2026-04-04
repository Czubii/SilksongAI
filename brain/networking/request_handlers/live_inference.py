from networking.client_services import ClientServices
from networking.handler_registry import register_handler
from networking.server import broadcast_event

@register_handler("initialize_live_inference")
async def set_inference_artifact(payload: dict, services: ClientServices):
    boss_name = payload["target_boss_name"]
    artifact_name = payload["artifact_name"]

    services.live_inference_service.select_artifact(boss_name, artifact_name)

@register_handler("live_inference")
async def live_inference(payload: dict, services: ClientServices):
    return services.live_inference_service.predict_inputs(payload)