import torch
from torch import nn
from ai.models import BaseBossNet, register_model
from shared import ModelDimensions


@register_model
class ExemplaryNet(BaseBossNet):
    def __init__(self,
                 base_dimensions: ModelDimensions,
                 time_window: int,
                 embedding_dim: int,
                 hidden_dim):
        super().__init__(base_dimensions)

        self.time_window = time_window
        self.embedding_dim = embedding_dim
        self.hidden_dim = hidden_dim

        self.embedding = nn.Embedding(base_dimensions.vocab_word_count, embedding_dim)
        self.embedded_size = base_dimensions.input_named_state * embedding_dim * time_window

        numeric_input = base_dimensions.input_continuous + base_dimensions.input_boolean

        self.continuous_projection = nn.Sequential(
            nn.Linear(numeric_input * time_window, hidden_dim),
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

    def forward(self, cont_input, bool_input, named_state):
        batch = cont_input.size(0)

        continuous = torch.cat(
            [cont_input, bool_input.float()],
            dim=-1
        )

        continuous = continuous.flatten(1)
        continuous_feat = self.continuous_projection(continuous)

        playmaker = self.embedding(named_state)
        playmaker = playmaker.flatten(1)

        playmaker_feat = self.playmaker_projection(playmaker)

        combined = torch.cat([continuous_feat, playmaker_feat], dim=1)
        x = self.trunk(combined)

        return {
            "output_continuous": self.output_continuous(x),
            "output_boolean": self.output_boolean(x)
        }

    def get_dict_config(self) -> dict:
        return {
            "model_type": self.__class__.__name__,
            "base_dims": self.base_dimensions,
            "time_window": self.time_window,
            "embedding_dim": self.embedding_dim,
            "hidden_dim": self.hidden_dim,
        }