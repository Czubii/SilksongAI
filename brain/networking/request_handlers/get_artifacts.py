from ai.models import get_available_artifacts_names, get_artifact_info
from networking.server import ClientServices
from networking.handler_registry import register_handler


@register_handler("get_artifacts")
async def get_artifacts(payload: dict, services: ClientServices):
    artifacts = get_available_artifacts_names()

    artifact_infos = {}

    for boss_name, artifacts in artifacts.items():
        artifact_infos[boss_name] = []

        for artifact in artifacts:
            artifact_infos[boss_name].append(get_artifact_info(boss_name, artifact))

    output = {
        "artifacts": artifact_infos
    }
    return output