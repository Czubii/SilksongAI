from ai.models import artifact_exists, ArtifactFactory, generate_artifact_path
from networking.services import services
from networking.handler_registry import register_handler


@register_handler("initialize_inference_session", True)
async def start_inference_session(payload: dict):
    boss_name = payload["target_boss_name"]
    artifact_name = payload["artifact_name"]

    if not artifact_exists(boss_name, artifact_name):
        raise Exception(f"Artifact does not exist: {boss_name}: {artifact_name}")

    artifact = ArtifactFactory.from_file(generate_artifact_path(boss_name, artifact_name))
    services.live_inference_service.set_artifact(artifact)

@register_handler("live_inference", True)
async def live_inference(payload: dict):
    return services.live_inference_service.predict_inputs(payload)