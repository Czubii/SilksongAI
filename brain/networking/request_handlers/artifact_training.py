import numpy as np

from ai.behavioral_cloning.behavioral_cloning_dataset import BossFightDataset
from ai.behavioral_cloning.trainer import BehaviorCloningTrainer
from ai.models import get_existing_artifact_paths, ArtifactFactory
from networking.client_services import ClientServices, BehavioralCloningTask
from networking.handler_registry import register_handler
from networking.server import broadcast_event


@register_handler("stop_training")
async def stop_training(payload: dict, services: ClientServices):
    if not services.is_task_running(BehavioralCloningTask):
        raise Exception("Task not running")

    services.task(BehavioralCloningTask).request_stop()

@register_handler("finalize_training")
async def finalize_training(payload: dict, services: ClientServices):
    if payload["save"]:
        artifact = services.task(BehavioralCloningTask).trainer.model_artifact
        path = get_existing_artifact_paths(artifact.boss_name, artifact.name).artifact
        services.task(BehavioralCloningTask).save_results(path)
    else:
        services.task(BehavioralCloningTask).discard_results()

@register_handler("set_artifact_config")
async def set_artifact_config(payload: dict, services: ClientServices):
    paths = get_existing_artifact_paths(payload["target_boss_name"], payload["artifact_name"])
    artifact = ArtifactFactory.from_file(paths.artifact)
    new_thresholds = payload["boolean_thresholds"]

    artifact.boolean_thresholds = np.array(new_thresholds)

    artifact.save(paths.artifact)

    await broadcast_event("new_artifact")

@register_handler("start_training")
async def train_artifact(payload: dict, services: ClientServices):
    if services.is_task_running(BehavioralCloningTask):
        raise Exception("Another training is still running")

    # Load artifact and datasets
    paths = get_existing_artifact_paths(payload["target_boss_name"], payload["artifact_name"])
    artifact = ArtifactFactory.from_file(paths.artifact)
    training_dataset = BossFightDataset(paths.training_dataset, artifact.model.time_window)
    testing_dataset = BossFightDataset(paths.testing_dataset, artifact.model.time_window)

    # Initialize trainer
    trainer = BehaviorCloningTrainer(artifact,
                                     training_dataset,
                                     testing_dataset,
                                     payload["learning_rate"],
                                     payload["batch_size"],
                                     payload["use_gpu"])

    num_epochs = payload["num_epochs"]
    services.task(BehavioralCloningTask).start(trainer=trainer,
                                               num_epochs=num_epochs)




