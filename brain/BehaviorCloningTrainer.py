import os
from typing import Optional
from sympy.printing.pytorch import torch
from torch import optim, nn
from torch.optim.lr_scheduler import ReduceLROnPlateau
from torch.utils.data import DataLoader

from BossFightDataset import BossFightDataset
from Networks import BossNet
from Preprocessing import ProcessingPipeline
from RecordingLayout import Layout, LayoutNotSupported


class BehaviorCloningTrainer:

    def __init__(self, model_name: str, model_time_widow: int = 5):

        self._model_name = model_name
        self._model_time_widow = model_time_widow
        self._dataset_loaded = False
        self._network_loaded = False

        self._training_dataset: Optional[BossFightDataset] = None
        self._testing_dataset: Optional[BossFightDataset] = None
        self._network: Optional[BossNet] = None


        self._working_dir = os.path.join("models", model_name)
        os.makedirs(self._working_dir, exist_ok=True)

        self._dataset_cont_dim = 0
        self._dataset_playmaker_dim = 0
        self._dataset_vocab_dim = 0

    def process_recordings(self, recording_path:str, target_boss:str, num_test_recordings: int) -> None:
        pp = ProcessingPipeline(recording_path, target_boss)
        pp.process_and_save(self._model_name, self._working_dir, num_test_recordings, save_vocab=True)
        return

    def load_dataset(self):
        training_file = os.path.join(self._working_dir, self._model_name + "_training.pt")
        if not os.path.exists(training_file):
            self._training_dataset = None
            raise FileNotFoundError(f"BehaviorCloningTrainer: Could not find training dataset.")

        training_data = torch.load(training_file)

        if training_data["layout_version"] != Layout.SUPPORTED_FORMAT_VERSION:
            raise LayoutNotSupported("BehaviorCloningTrainer: Training dataset uses different layout format than currently supported")

        self._training_dataset = BossFightDataset(training_data, self._model_time_widow)
        self._dataset_cont_dim = training_data["cont_dim"]
        self._dataset_playmaker_dim = training_data["playmaker_dim"]
        self._dataset_vocab_dim = training_data["vocab_dim"]

        self._dataset_loaded = True

        testing_file = os.path.join(self._working_dir, self._model_name + "_testing.pt")
        if not os.path.exists(testing_file):
            print("BehaviorCloningTrainer: Could not find testing dataset. Continuing without it.")
            return

        testing_data = torch.load(testing_file)

        if training_data["layout_version"] != Layout.SUPPORTED_FORMAT_VERSION:
            print("BehaviorCloningTrainer: Testing dataset uses different layout format than currently supported. Continuing without it.")
            self._testing_dataset = None
            return

        if(testing_data["cont_dim"] != self._dataset_cont_dim
        or testing_data["playmaker_dim"] != self._dataset_playmaker_dim
        or testing_data["vocab_dim"] != self._dataset_vocab_dim):
            print("BehaviorCloningTrainer: Could not load testing dataset - the dimensions dont fit the training data. Continuing without it.")
            self._testing_dataset = None
            return

        self._testing_dataset = BossFightDataset(testing_data)


    def create_network(self, hidden_dim, embedding_dim: int = None):
        if not self._dataset_loaded:
            raise Exception("BehaviorCloningTrainer: To create new network you have to load the dataset first.")

        if embedding_dim is None:
            embedding_dim = int(pow(self._dataset_vocab_dim, 0.25))

        self._network = BossNet(self._model_time_widow,
                                self._dataset_cont_dim,
                                self._dataset_playmaker_dim,
                                self._dataset_vocab_dim,
                                embedding_dim,
                                hidden_dim,
                                Layout.Inputs.num_elements)

        self._network_loaded = True

    def train_network(self, num_epochs: int,
                      learning_rate: float = 1e-4,
                      batch_size: int = 32,
                      use_gpu: bool = False,):

        if not self._network_loaded:
            print("BehaviorCloningTrainer: Network not loaded. Cannot start training.")

        device = "cpu"

        if use_gpu:
            if torch.cuda.is_available():
                device = torch.device("cuda")
            else:
                print("CUDA device not available. Make sure you have the correct pytorch version installed and your device supports CUDA computation.")
        print("Starting training. Using device:", device)


        optimizer = optim.Adam(self._network.parameters(), lr=learning_rate)
        criterion = nn.MSELoss()

        dataloader = DataLoader(self._training_dataset, batch_size=batch_size, shuffle=True)
        dataloader_test = DataLoader(self._testing_dataset, batch_size=batch_size, shuffle=True)

        for epoch in range(num_epochs):

            total_loss = 0
            n = 0
            for continuous_batch, playmaker_batch, target_batch in dataloader:
                continuous_batch = continuous_batch.to(device, dtype=torch.float32)
                playmaker_batch = playmaker_batch.to(device, dtype=torch.long)
                target_batch = target_batch.to(device, dtype=torch.float32)

                optimizer.zero_grad()
                output_batch = self._network(continuous_batch, playmaker_batch)
                loss = criterion(output_batch, target_batch)
                loss.backward()
                optimizer.step()

                total_loss += loss.item()
                n += 1

            if self._testing_dataset is None:
                print(f"Epoch: {epoch} | Avg Loss: {total_loss / n}")
                continue

            total_loss_testing = 0
            m = 0
            for continuous_batch, playmaker_batch, target_batch in dataloader_test:
                output_batch = self._network(continuous_batch, playmaker_batch)
                total_loss_testing += criterion(output_batch, target_batch)
                m+=1

            print(f"Epoch: {epoch} | Avg Loss: {total_loss / n} | Testing data avg loss: {total_loss_testing / m}")


if __name__ == '__main__':
    pipeline = BehaviorCloningTrainer("lacetest")

    try:
        pipeline.load_dataset()
    except FileNotFoundError as e:
        print(e)
        print("Creating new dataset")
        pipeline.process_recordings(recording_path="../recordings/Lace Boss1",
                                    target_boss="Lace Boss1",
                                    num_test_recordings=1)
        pipeline.load_dataset()

    pipeline.create_network(100)

    pipeline.train_network(100)
