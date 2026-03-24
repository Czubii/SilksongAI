from ai.behavioral_cloning.behavioral_cloning_dataset import BossFightDataset
from ai.behavioral_cloning.trainer import BehaviorCloningTrainer
from ai.models import get_artifact_paths, ArtifactFactory
from networking.client_services import ClientServices, BehavioralCloningTask
from networking.handler_registry import register_handler


@register_handler("stop_training")
async def stop_training(payload: dict, services: ClientServices):
    if not services.is_task_running(BehavioralCloningTask):
        raise Exception("Task not running")

    services.task(BehavioralCloningTask).request_stop()

@register_handler("finalize_training")
async def finalize_training(payload: dict, services: ClientServices):
    if services.task(BehavioralCloningTask).results_handled:
        raise Exception("Nothing to save.")

    if payload["save"]:
        services.task(BehavioralCloningTask).save_results()
    else:
        services.task(BehavioralCloningTask).discard_results()


@register_handler("start_training")
async def train_artifact(payload: dict, services: ClientServices):
    if services.is_task_running(BehavioralCloningTask):
        raise Exception("Another training is still running")

    # Load artifact and datasets
    paths = get_artifact_paths(payload["target_boss_name"], payload["artifact_name"])
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




