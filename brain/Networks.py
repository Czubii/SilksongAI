import torch
from torch import nn, optim
from torch.utils.data import DataLoader
from BossFightDataset import BossFightDataset
from RecordingLayout import Layout


class BossModelArtifact:
    def __init__(
        self,
        model: nn.Module,
        boss_name: str,
        vocab: dict,
        metadata: dict,
    ):
        self.model: BossNet = model
        self.boss_name = boss_name
        self.vocab = vocab
        self.metadata = metadata

        self.mean = metadata["transformations"]["cont_mean"]
        self.std = metadata["transformations"]["cont_std"]

    def save(self, path):
        torch.save({
            "format_version": Layout.SUPPORTED_FORMAT_VERSION,
            "state_dict": self.model.state_dict(),
            "config": self.model.config,
            "boss_name": self.boss_name,
            "vocab": self.vocab,
            "metadata": self.metadata,
        }, path)

    def dataset_matches(self, dataset: BossFightDataset) -> bool:
        if dataset.target_boss == self.boss_name:
            return True

        return False

class BossModelFactory:
    @staticmethod
    def load(path: str, device: str = "cpu") -> BossModelArtifact:
        checkpoint = torch.load(path, map_location=device, weights_only=False)

        # Optional: version check
        if checkpoint.get("format_version", 1) != Layout.SUPPORTED_FORMAT_VERSION:
            raise ValueError("Unsupported model format version")

        config = checkpoint["config"]

        # Recreate network
        model = BossNet(**config)
        model.load_state_dict(checkpoint["state_dict"])
        model.to(device)
        model.eval()

        return BossModelArtifact(
            model=model,
            boss_name=checkpoint["boss_name"],
            vocab=checkpoint["vocab"],
            metadata=checkpoint["metadata"],
        )

class BossNet(nn.Module):
    def __init__(
            self,
            time_window: int,
            cont_dim: int,
            playmaker_dim: int,
            vocab_dim: int,
            embedding_dim: int,
            hidden_dim: int,
            output_dim: int):

        super().__init__()

        self.config = {
            "time_window": time_window,
            "cont_dim": cont_dim,
            "playmaker_dim": playmaker_dim,
            "vocab_dim": vocab_dim,
            "embedding_dim": embedding_dim,
            "hidden_dim": hidden_dim,
            "output_dim": output_dim
        }

        self.embedding = nn.Embedding(vocab_dim, embedding_dim)
        self.embedded_size = playmaker_dim * embedding_dim * time_window

        self.continuous_projection = nn.Sequential(
                nn.Linear(cont_dim * time_window, hidden_dim),
                nn.ReLU()
        )

        self.playmaker_projection = nn.Sequential(
            nn.Linear(self.embedded_size, hidden_dim),
            nn.ReLU()
        )

        self.trunk = nn.Sequential(
            nn.Linear(2*hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU()
        )

        self.output = nn.Linear(hidden_dim, output_dim)

    def forward(self, continuous, playmaker):

        batch_size = continuous.size(0)
        continuous = continuous.view(batch_size, -1) # flatten
        continuous_feat = self.continuous_projection(continuous) #project

        playmaker_embeddings = self.embedding(playmaker)
        playmaker_embeddings = playmaker_embeddings.view(batch_size, -1)

        playmaker_feat = self.playmaker_projection(playmaker_embeddings)

        combined = torch.cat([continuous_feat, playmaker_feat], dim=1)
        x = self.trunk(combined)

        return self.output(x)

