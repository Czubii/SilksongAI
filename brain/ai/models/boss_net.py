import torch
from torch import nn
from model_dimensions import ModelDimensions
from ai.base_model import BaseBossNet
from ai.registry import register_model


@register_model
class BossNet(BaseBossNet):
    def __init__(self,
                 base_dimensions: ModelDimensions,
                 time_window: int,
                 embedding_dim: int,
                 hidden_dim):
        super().__init__()

        self.embedding = nn.Embedding(base_dimensions.vocab_word_count, embedding_dim)
        self.embedded_size = base_dimensions.input_named_state * embedding_dim * time_window

        self.continuous_projection = nn.Sequential(
            nn.Linear(base_dimensions.input_continuous * time_window, hidden_dim),
            nn.ReLU()
        )

        self.playmaker_projection = nn.Sequential(
            nn.Linear(self.embedded_size, hidden_dim),
            nn.ReLU()
        )

        self.trunk = nn.Sequential(
            nn.Linear(2 * hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU()
        )
        self.output_continuous = nn.Linear(hidden_dim, base_dimensions.output_continuous)
        self.output_boolean = nn.Linear(hidden_dim, base_dimensions.output_boolean)

    def forward(self, continuous, playmaker):
        batch_size = continuous.size(0)
        continuous = continuous.view(batch_size, -1)  # flatten
        continuous_feat = self.continuous_projection(continuous)  #project

        playmaker_embeddings = self.embedding(playmaker)
        playmaker_embeddings = playmaker_embeddings.view(batch_size, -1)

        playmaker_feat = self.playmaker_projection(playmaker_embeddings)

        combined = torch.cat([continuous_feat, playmaker_feat], dim=1)
        x = self.trunk(combined)

        return self.output(x)

    def get_dict_config(self) -> dict:
        return {
            "type": self.__class__.__name__,
            "base_dimensions": self.base_dimensions,
            "time_window": self.time_window,
            "embedding_dim": self.embedding_dim,
            "hidden_dim": self.hidden_dim,
        }