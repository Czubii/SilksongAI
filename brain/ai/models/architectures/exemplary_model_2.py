import torch
from torch import nn
from ai.models import BaseBossNet, register_model
from shared import ModelDimensions


@register_model
class StructuredBossNet(BaseBossNet):

    def __init__(
        self,
        base_dimensions: ModelDimensions,
        time_window: int = 5,
        embedding_dim: int = 24,
        entity_dim: int = 128,
        rnn_hidden: int = 192,
        rnn_layers: int = 2
    ):

        super().__init__(
            base_dimensions,
            time_window=time_window,
            embedding_dim=embedding_dim,
            entity_dim=entity_dim,
            rnn_hidden=rnn_hidden,
            rnn_layers=rnn_layers
        )

        self.time_window = time_window

        numeric_input = (
            base_dimensions.input_continuous +
            base_dimensions.input_boolean
        )

        # -------------------------
        # Boss intention embedding
        # -------------------------

        self.embedding = nn.Embedding(
            base_dimensions.vocab_word_count,
            embedding_dim
        )

        self.boss_state_encoder = nn.Sequential(
            nn.Linear(base_dimensions.input_named_state * embedding_dim, entity_dim),
            nn.ReLU(),
            nn.LayerNorm(entity_dim)
        )

        # -------------------------
        # Physics encoder
        # -------------------------

        self.physics_encoder = nn.Sequential(
            nn.Linear(numeric_input, entity_dim),
            nn.ReLU(),
            nn.LayerNorm(entity_dim),
            nn.Linear(entity_dim, entity_dim),
            nn.ReLU()
        )

        # -------------------------
        # Feature fusion
        # -------------------------

        self.fusion = nn.Sequential(
            nn.Linear(entity_dim * 2, entity_dim),
            nn.ReLU()
        )

        # -------------------------
        # Temporal module
        # -------------------------

        self.rnn = nn.GRU(
            input_size=entity_dim,
            hidden_size=rnn_hidden,
            num_layers=rnn_layers,
            batch_first=True
        )

        # -------------------------
        # Decision trunk
        # -------------------------

        self.trunk = nn.Sequential(
            nn.Linear(rnn_hidden, rnn_hidden),
            nn.ReLU(),
            nn.LayerNorm(rnn_hidden)
        )

        # -------------------------
        # Action heads
        # -------------------------

        self.output_continuous = nn.Linear(
            rnn_hidden,
            base_dimensions.output_continuous
        )

        self.output_boolean = nn.Linear(
            rnn_hidden,
            base_dimensions.output_boolean
        )

    def forward(self, cont_input, bool_input, named_state):

        numeric = torch.cat(
            [cont_input, bool_input.float()],
            dim=-1
        )

        physics_feat = self.physics_encoder(numeric)

        embedded = self.embedding(named_state)
        embedded = embedded.flatten(-2)

        boss_feat = self.boss_state_encoder(embedded)

        fused = torch.cat([physics_feat, boss_feat], dim=-1)
        fused = self.fusion(fused)

        rnn_out, _ = self.rnn(fused)

        x = rnn_out[:, -1]

        x = self.trunk(x)

        return {
            "output_continuous": self.output_continuous(x),
            "output_boolean": self.output_boolean(x)
        }