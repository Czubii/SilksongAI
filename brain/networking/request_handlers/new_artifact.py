from ai.models import BaseBossNet, prepare_dataset_and_artifact
from data_processing import ChoosePercentBest, RecordingFilters
from networking.server import ClientServices, broadcast_event
from networking.handler_registry import register_handler


@register_handler("new_artifact")
async def new_artifact(payload: dict, services: ClientServices):
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

    await broadcast_event("new_artifact")