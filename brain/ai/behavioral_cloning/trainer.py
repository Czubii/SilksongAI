import io
import threading

import matplotlib.pyplot as plt
import numpy as np
from sympy.printing.pytorch import torch
from torch import optim, nn
from torch.utils.data import DataLoader
from ai.behavioral_cloning.behavioral_cloning_dataset import BossFightDataset
from ai.models import Artifact
from ai.models.artifact_manager import *

class BehaviorCloningTrainer:
    def __init__(self,
                 artifact: Artifact,
                 training_dataset: BossFightDataset,
                 testing_dataset: BossFightDataset,
                 learning_rate: float = 5e-4,
                 batch_size: int = 32,
                 use_gpu: bool = False):

        self.model_artifact = artifact
        self._training_dataset = training_dataset
        self._testing_dataset = testing_dataset

        self.optimizer = optim.Adam(self.model_artifact.model.parameters())

        self.regression_loss = nn.SmoothL1Loss()
        self.binary_loss = nn.BCEWithLogitsLoss(pos_weight=self._training_dataset.pos_weights)#


        self.device: str = "cpu"
        if use_gpu and torch.cuda.is_available():
            self.device = torch.device("cuda")

        print("Starting training. Using device:", self.device)

        if self.model_artifact.metadata["behavioral_cloning"] is None:
            self.model_artifact.metadata["behavioral_cloning"] = BehaviorCloningTrainer.get_empty_metadata()

        self.model_artifact.model.to(self.device)

        for g in self.optimizer.param_groups:
            g['lr'] = learning_rate

        self.scheduler = torch.optim.lr_scheduler.StepLR(self.optimizer, step_size=10, gamma=0.5)#TODO add as parameter
        self.regression_loss.to(self.device)
        self.binary_loss.to(self.device)

        self.training_dataloader = DataLoader(self._training_dataset, batch_size=batch_size, shuffle=True)
        self.testing_dataloader = DataLoader(self._testing_dataset, batch_size=batch_size, shuffle=False)

    def run_epoch(self) -> dict:
        self.model_artifact.model.train()
        training_loss = self._epoch(self.training_dataloader, False)

        self.model_artifact.model.eval()
        with torch.no_grad():
            testing_loss = self._epoch(self.testing_dataloader, True)

        self.scheduler.step()

        self.model_artifact.metadata["behavioral_cloning"]["total_epochs"] += 1
        self.model_artifact.metadata["behavioral_cloning"]["losses"].append((training_loss, testing_loss))

        return {
            "testing_loss": testing_loss,
            "training_loss": training_loss,
            "current_epoch": self.model_artifact.metadata["behavioral_cloning"]["total_epochs"]
        }

    @staticmethod
    def get_empty_metadata() -> dict:
        return {
            "total_epochs": 0,
            "losses": [],
        }

    def _epoch(self, dataloader: DataLoader, testing: bool = False) -> float:
        """
        :param dataloader:
        :return: average loss for this epoch
        """
        total_loss = 0
        n = 0
        for dataset_entry in dataloader:
            continuous_batch = dataset_entry["input_continuous"].to(self.device, dtype=torch.float32)
            boolean_batch = dataset_entry["input_boolean"].to(self.device, dtype=torch.float32)
            input_named_state_batch = dataset_entry["input_named_state"].to(self.device, dtype=torch.long)

            target_continuous_batch = dataset_entry["output_continuous"].to(self.device, dtype=torch.float32)
            target_boolean_batch = dataset_entry["output_boolean"].to(self.device, dtype=torch.float32)

            self.optimizer.zero_grad()
            output_batch = self.model_artifact.model(continuous_batch, boolean_batch, input_named_state_batch)

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


