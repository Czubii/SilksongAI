import numpy as np
from ai.models import get_available_artifacts_names, get_artifact_info, get_existing_artifact_paths, ArtifactFactory, \
    prepare_dataset_and_artifact, BaseBossNet
from data_processing import RecordingFilters, ChoosePercentBest
from networking.handler_registry import register_handler
from networking.client_manager import client_manager

@register_handler("set_artifact_config", True)
async def set_artifact_config(payload: dict):
    paths = get_existing_artifact_paths(payload["target_boss_name"], payload["artifact_name"])
    artifact = ArtifactFactory.from_file(paths.artifact)
    new_thresholds = payload["boolean_thresholds"]

    artifact.boolean_thresholds = np.array(new_thresholds)

    artifact.save(paths.artifact)

    await client_manager.send_host_event("artifacts_modified")


@register_handler("get_artifacts", False)
async def get_artifacts(payload: dict):
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

@register_handler("new_artifact", True)
async def new_artifact(payload: dict):
    quality_filter = ChoosePercentBest(payload["percent_best"])
    filters = RecordingFilters(payload["require_success"],
                               payload["delta"],
                               payload["player_name"],
                               quality_filter)

    network_params = BaseBossNet.deserialize_params(payload["params"])

    prepare_dataset_and_artifact(payload["target_boss"],
                                 payload["name"],
                                 filters,
                                 payload["overwrite"],
                                 0.2,
                                 payload["architecture"],
                                 **network_params)

    await client_manager.send_host_event("artifacts_modified")