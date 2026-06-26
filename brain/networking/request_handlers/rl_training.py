# from ai.models import get_existing_artifact_paths, ArtifactFactory
# from ai.reinforced_learning.trainer import RLTrainer
# from networking.services import ServiceRegistry
# from networking.handler_registry import register_handler

# @register_handler("rl_start")
# async def start_rl(payload: dict, services: ServiceRegistry):
#     if services.is_any_task_running():
#         raise Exception("Another task is still running")
#
#     # Load artifact and datasets
#     paths = get_existing_artifact_paths(payload["target_boss_name"], payload["artifact_name"])
#     artifact = ArtifactFactory.from_file(paths.artifact)
#
#     trainer = RLTrainer(artifact)
#
#     num_epochs = payload["num_epochs"]
#     fights_per_epoch = payload["fights_per_epoch"]
#
#     services.live_inference_service.set_artifact(artifact)
#
#     services.task(RLOrchestrator).start(trainer=trainer,
#                                         num_epochs=num_epochs,
#                                         fights_per_epoch = fights_per_epoch)
#
# @register_handler("rl_run_fight")
# async def start_rl(payload: dict, services: ServiceRegistry):
#     if services.is_task_running(RLOrchestrator) and services.task(RLOrchestrator).any_fights_remaining():
#         return {"can_run": True, "target_boss_name": services.task(RLOrchestrator).trainer.model_artifact.boss_name}
#
#     return {"can_run": False}
#
# @register_handler("rl_fight_finished")
# async def start_rl(payload: dict, services: ServiceRegistry):
#     if not services.is_task_running(RLOrchestrator):
#         raise Exception("Reinforcement Learning is not running!!")
#
#     services.task(RLOrchestrator).on_fight_finished()