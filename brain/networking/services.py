import asyncio
import threading
import traceback
from abc import abstractmethod, ABC
from asyncio import Task
from pathlib import Path
from typing import Optional, TypeVar

import msgpack

from ai.behavioral_cloning.trainer import BCTrainer
from ai.live_inference.live_inference_service import LiveInferenceService
from client_manager import client_manager

class ThreadedTask(ABC):
    def __init__(self):
        self.task: Optional[Task] = None
        self.stop_event = threading.Event()

    def start(self, *args, **kwargs):
        if self.task and not self.task.done():
            raise Exception("Task is already running!")

        can_start, msg = self._can_start(*args, **kwargs)
        if not can_start:
            raise Exception(f"Task not ready: {msg}")

        self.stop_event.clear()

        self._on_start(*args, **kwargs)

        loop = asyncio.get_running_loop()
        self.task = asyncio.create_task(
            asyncio.to_thread(self._task_loop, *args, loop=loop, **kwargs))

    def request_stop(self):
        if not self.stop_event.is_set():
            self.stop_event.set()

    def is_running(self) -> bool:
        return self.task and not self.task.done()

    @abstractmethod
    def _task_loop(self, *args, loop, **kwargs):
        pass

    @abstractmethod
    def _on_start(self, *args, **kwargs):
        pass

    @abstractmethod
    def _can_start(self, *args, **kwargs) -> (bool, str):
        pass

class BehavioralCloningTask(ThreadedTask):
    def __init__(self):
        super().__init__()
        self.trainer: Optional[BCTrainer] = None
        self.results_handled = True

    def _can_start(self, *args, **kwargs):
        if not self.results_handled:
            return False, "results not handled"

        return True, ""

    def save_results(self, path: Path):
        self.trainer.model_artifact.save(path)
        self.results_handled = True

    def discard_results(self):
        self.results_handled = True

    def _on_start(self, trainer, num_epochs):
        self.results_handled = False
        self.trainer = trainer # needed for saving

    def _task_loop(self, trainer: BCTrainer, num_epochs: int, *, loop):
        try:
            start_epoch = trainer.model_artifact.metadata["behavioral_cloning"]["total_epochs"]
            end_epoch = start_epoch + num_epochs

            for epoch in range(num_epochs):
                print(f"Epoch: {epoch}")
                if self.stop_event.is_set():
                    break

                epoch_info = trainer.run_epoch()
                epoch_info["end_epoch"] = end_epoch
                epoch_info["start_epoch"] = start_epoch
                epoch_info["loss_history"] = trainer.model_artifact.metadata["behavioral_cloning"]["losses"]
                print(epoch_info)
                asyncio.run_coroutine_threadsafe(client_manager.send_host_event("bc_epoch", epoch_info), loop)
        except Exception as e:
            print(f"Training failed: {e}")
        finally:
            asyncio.run_coroutine_threadsafe(client_manager.send_host_event("bc_finished"), loop)

T = TypeVar("T", bound="ThreadedTask")
class ServiceRegistry:
    def __init__(self):
        self.live_inference_service = LiveInferenceService()
        self._tasks: dict[type[ThreadedTask], ThreadedTask] = {}
        self._initialize_tasks()

    def _initialize_tasks(self):
        self._tasks[BehavioralCloningTask] = BehavioralCloningTask()

    def task(self, task_type: type[T]) -> T:
        return self._tasks[task_type]

    def is_task_running(self, task: type[ThreadedTask]) -> bool:
        return self._tasks[task].is_running()

    def is_any_task_running(self) -> bool:
        for task in self._tasks.values():
            if task.is_running(): return True

        return False



services = ServiceRegistry()