import numpy as np

from ai.behavioral_cloning.behavioral_cloning_dataset import BossFightDataset
from ai.behavioral_cloning.trainer import BCTrainer
from ai.models import get_existing_artifact_paths, ArtifactFactory
from networking.services import BehavioralCloningTask, services
from networking.handler_registry import register_handler



@register_handler("bc_stop", True)
async def stop_training(payload: dict):
    if not services.is_task_running(BehavioralCloningTask):
        raise Exception("Task not running")

    services.task(BehavioralCloningTask).request_stop()

@register_handler("bc_finalize", True)
async def finalize_training(payload: dict):
    if payload["save"]:
        artifact = services.task(BehavioralCloningTask).trainer.model_artifact
        path = get_existing_artifact_paths(artifact.boss_name, artifact.name).artifact
        services.task(BehavioralCloningTask).save_results(path)
    else:
        services.task(BehavioralCloningTask).discard_results()

@register_handler("bc_start", True)
async def train_artifact(payload: dict):
    if services.is_any_task_running():
        raise Exception("Another task is still running")

    # Load artifact and datasets
    paths = get_existing_artifact_paths(payload["target_boss_name"], payload["artifact_name"])
    artifact = ArtifactFactory.from_file(paths.artifact)
    training_dataset = BossFightDataset(paths.training_dataset, artifact.model.time_window)
    testing_dataset = BossFightDataset(paths.testing_dataset, artifact.model.time_window)

    # Initialize trainer
    trainer = BCTrainer(artifact,
                        training_dataset,
                        testing_dataset,
                        payload["learning_rate"],
                        payload["batch_size"],
                        payload["use_gpu"])

    num_epochs = payload["num_epochs"]
    services.task(BehavioralCloningTask).start(trainer=trainer,
                                               num_epochs=num_epochs)




