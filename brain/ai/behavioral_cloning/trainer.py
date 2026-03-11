import matplotlib.pyplot as plt
import numpy as np
from sympy.printing.pytorch import torch
from torch import optim, nn
from torch.utils.data import DataLoader
from ai.behavioral_cloning.behavioral_cloning_dataset import BossFightDataset
from ai.models import Artifact
from ai.models.artifact_manager import *

class BehaviorCloningTrainer:
    def __init__(self, artifact: Artifact, training_dataset: BossFightDataset, testing_dataset: BossFightDataset):
        self._model_artifact = artifact
        self._training_dataset = training_dataset
        self._testing_dataset = testing_dataset

        self.optimizer = optim.Adam(self._model_artifact.model.parameters())
        self.scheduler = torch.optim.lr_scheduler.StepLR(self.optimizer, step_size=10, gamma=0.5) #TODO change

        self.regression_loss = nn.SmoothL1Loss()
        self.binary_loss = nn.BCEWithLogitsLoss(pos_weight=self._training_dataset.pos_weights)#
        #TODO add some dataset vs network dimension validation

    def train_network(self, num_epochs: int,
                      learning_rate: float = 5e-4,
                      batch_size: int = 32,
                      use_gpu: bool = False,):

        device = "cpu"
        if use_gpu:
            if torch.cuda.is_available():
                device = torch.device("cuda")
            else:
                print("CUDA device not available. Make sure you have the correct pytorch version installed and your device supports CUDA computation.")
        print("Starting training. Using device:", device)

        if self._model_artifact.metadata["behavioral_cloning"] is None:
            self._model_artifact.metadata["behavioral_cloning"] = BehaviorCloningTrainer.get_empty_metadata()

        artifact_starting_epoch = self._model_artifact.metadata["behavioral_cloning"]["total_epochs"]

        self._model_artifact.model.to(device)

        for g in self.optimizer.param_groups:
            g['lr'] = learning_rate
        self.regression_loss.to(device)
        self.binary_loss.to(device)

        training_dataloader = DataLoader(self._training_dataset, batch_size=batch_size, shuffle=True)
        testing_dataloader = DataLoader(self._testing_dataset, batch_size=batch_size, shuffle=False)

        for epoch in range(num_epochs):

            self._model_artifact.model.train()
            training_loss = self._epoch(training_dataloader, device, False)

            self._model_artifact.model.eval()
            with torch.no_grad():
                testing_loss = self._epoch(testing_dataloader, device, True)

            self.scheduler.step()

            self._model_artifact.metadata["behavioral_cloning"]["total_epochs"] += 1
            self._model_artifact.metadata["behavioral_cloning"]["losses"].append((training_loss, testing_loss))

            print(f"Epoch {artifact_starting_epoch + epoch + 1}/{artifact_starting_epoch +num_epochs}. "
                  f"Training loss: {training_loss:.4f}. Testing loss: {testing_loss:.4f}")

    @staticmethod
    def get_empty_metadata() -> dict:
        return {
            "total_epochs": 0,
            "losses": [],
        }

    def _epoch(self, dataloader: DataLoader, device: str, testing: bool = False) -> float:
        """
        :param dataloader:
        :return: average loss for this epoch
        """
        total_loss = 0
        n = 0
        for dataset_entry in dataloader:
            continuous_batch = dataset_entry["input_continuous"].to(device, dtype=torch.float32)
            boolean_batch = dataset_entry["input_boolean"].to(device, dtype=torch.float32)
            input_named_state_batch = dataset_entry["input_named_state"].to(device, dtype=torch.long)

            target_continuous_batch = dataset_entry["output_continuous"].to(device, dtype=torch.float32)
            target_boolean_batch = dataset_entry["output_boolean"].to(device, dtype=torch.float32)

            self.optimizer.zero_grad()
            output_batch = self._model_artifact.model(continuous_batch, boolean_batch, input_named_state_batch)

            prediction_continuous_batch = output_batch["output_continuous"]
            prediction_boolean_batch = output_batch["output_boolean"]

            loss_float = self.regression_loss(prediction_continuous_batch, target_continuous_batch)
            loss_bool = self.binary_loss(prediction_boolean_batch, target_boolean_batch)

            loss = 0.7 * loss_float + 0.3 * loss_bool

            if not testing:
                loss.backward()
                self.optimizer.step()

            total_loss += loss.item()
            n += 1

        return total_loss / n

    def plot_losses(self):

        if self._model_artifact.metadata["behavioral_cloning"] is None:
            print("Model has not been trained yet.")
            return

        losses = self._model_artifact.metadata["behavioral_cloning"].get("losses", [(0, 0)])
        y_training, y_testing = zip(*losses)
        x = np.arange(0, len(y_training))

        plt.plot(x, y_training, label="Training Loss", marker='x')
        plt.plot(x, y_testing, label="Testing Loss", marker='x')
        plt.grid(True)
        plt.xlabel("Epoch")
        plt.ylabel("Loss")
        plt.legend()
        plt.show()

if __name__ == '__main__':
    paths = get_artifact_paths("Lace Boss1", "test1")
    artifact = ArtifactFactory.from_file(paths.artifact)
    training_dataset = BossFightDataset(paths.training_dataset, artifact.model.time_window)
    testing_dataset = BossFightDataset(paths.testing_dataset, artifact.model.time_window)

    pipeline = BehaviorCloningTrainer(artifact, training_dataset, testing_dataset)
    pipeline.train_network(num_epochs=10, learning_rate=5e-4, batch_size=8, use_gpu=True)


    artifact.save(paths.artifact)

    pipeline.plot_losses()

