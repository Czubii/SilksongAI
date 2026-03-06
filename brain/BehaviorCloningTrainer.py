import os
from sympy.printing.pytorch import torch
from torch import optim, nn
from torch.utils.data import DataLoader
from BossFightDataset import BossFightDataset
from Networks import BossNet, BossModelArtifact, BossModelFactory
from Preprocessing import ProcessingPipeline
from RecordingLayout import Layout, LayoutNotSupported
import matplotlib.pyplot as plt
from model_manager import model_exists, get_model_root_path, get_model_dataset_path, get_model_path


class BehaviorCloningTrainer:

    def __init__(self, target_boss_name: str, model_name: str):
        if not model_exists(target_boss_name, model_name):
            raise NameError("such model does not exist")


        self.model_path = get_model_path(target_boss_name, model_name)
        self._model_root_path = get_model_root_path(target_boss_name, model_name)
        self._dataset_path = get_model_dataset_path(target_boss_name, model_name)

        self._model_artifact = BossModelFactory.load(self.model_path)

        model_time_window = self._model_artifact.model.config["time_window"]

        training_file = os.path.join(self._dataset_path, "training_data.pt")
        testing_file = os.path.join(self._dataset_path, "testing_data.pt")
        if not os.path.exists(training_file) or not os.path.exists(testing_file):
            raise FileNotFoundError(f"BehaviorCloningTrainer: Could not find training dataset.")

        training_data = torch.load(training_file)
        testing_data = torch.load(testing_file)

        self._training_dataset = BossFightDataset(training_data, model_time_window)
        self._testing_dataset = BossFightDataset(testing_data, model_time_window)

        if (not self._model_artifact.dataset_matches(self._training_dataset) or
            not self._model_artifact.dataset_matches(self._testing_dataset)):
            raise Exception("Dataset does not match the network")

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

        self._model_artifact.model.to(device)

        optimizer = optim.Adam(self._model_artifact.model.parameters(), lr=learning_rate)

        regression_loss = nn.SmoothL1Loss().to(device)
        binary_loss = nn.BCEWithLogitsLoss(pos_weight=torch.tensor([35, 20, 15, 13, 15, 15])).to(device)

        scheduler = torch.optim.lr_scheduler.StepLR(optimizer, step_size=10, gamma=0.5)

        dataloader = DataLoader(self._training_dataset, batch_size=batch_size, shuffle=True)
        dataloader_test = DataLoader(self._testing_dataset, batch_size=batch_size, shuffle=True)

        training_loss_per_epoch = []
        testing_loss_per_epoch = []

        for epoch in range(num_epochs):

            total_loss_testing = 0
            m = 0
            for continuous_batch, playmaker_batch, target_batch in dataloader_test:
                continuous_batch = continuous_batch.to(device, dtype=torch.float32)
                playmaker_batch = playmaker_batch.to(device, dtype=torch.long)
                target_batch = target_batch.to(device, dtype=torch.float32)
                with torch.no_grad():
                    output_batch = self._model_artifact.model(continuous_batch, playmaker_batch)

                pred_float = output_batch[:, :Layout.Inputs.float_values]
                pred_bool = output_batch[:, Layout.Inputs.float_values:]

                target_float = target_batch[:, :Layout.Inputs.float_values]
                target_bool = target_batch[:, Layout.Inputs.float_values:]

                loss_float = regression_loss(pred_float, target_float)
                loss_bool = binary_loss(pred_bool, target_bool)

                total_loss_testing += (0.7 * loss_float + 0.3 * loss_bool).item()

                m += 1

            total_loss = 0
            n = 0
            for continuous_batch, playmaker_batch, target_batch in dataloader:
                continuous_batch = continuous_batch.to(device, dtype=torch.float32)
                playmaker_batch = playmaker_batch.to(device, dtype=torch.long)
                target_batch = target_batch.to(device, dtype=torch.float32)

                optimizer.zero_grad()
                output_batch = self._model_artifact.model(continuous_batch, playmaker_batch)

                pred_float = output_batch[:, :Layout.Inputs.float_values]
                pred_bool = output_batch[:, Layout.Inputs.float_values:]

                target_float = target_batch[:, :Layout.Inputs.float_values]
                target_bool = target_batch[:, Layout.Inputs.float_values:]
                loss_float = regression_loss(pred_float, target_float)
                loss_bool = binary_loss(pred_bool, target_bool)

                loss = 0.7 * loss_float + 0.3 * loss_bool

                loss.backward()
                optimizer.step()

                total_loss += loss.item()
                n += 1

            scheduler.step()

            training_loss_per_epoch.append(total_loss / n)
            testing_loss_per_epoch.append(total_loss_testing / m)
            print(f"Epoch: {epoch} | Avg Loss: {total_loss / n:.4} | Testing data avg loss: {total_loss_testing / m:.4}")

            self._model_artifact.metadata["epochs"] += 1
            self._model_artifact.metadata["loss"] = total_loss_testing / m

        return training_loss_per_epoch, testing_loss_per_epoch

    def save_model(self):
        self._model_artifact.save(self.model_path)




def plot_training_testing_loss(training_loss_per_epoch, testing_loss_per_epoch, title="Training vs Testing Loss"):
    epochs = range(1, len(training_loss_per_epoch) + 1)

    plt.figure(figsize=(8, 5))
    plt.plot(epochs, training_loss_per_epoch, label="Training Loss", marker='o')
    plt.plot(epochs, testing_loss_per_epoch, label="Testing Loss", marker='x')

    plt.xlabel("Epoch")
    plt.ylabel("Loss")
    plt.title(title)
    plt.grid(True, linestyle='--', alpha=0.5)
    plt.legend()
    plt.tight_layout()
    plt.show()

if __name__ == '__main__':
    pipeline = BehaviorCloningTrainer("Lace Boss1", "notsucker_35v2")


    training_loss_per_epoch, testing_loss_per_epoch = (pipeline.train_network(15,
                               use_gpu=True,
                               batch_size=64,
                               learning_rate = 1e-3))

    plot_training_testing_loss(training_loss_per_epoch, testing_loss_per_epoch,)

    pipeline.save_model()