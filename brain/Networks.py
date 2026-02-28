import torch
from torch import nn, optim
from torch.utils.data import DataLoader
from BossFightDataset import BossFightDataset

class BossNet(nn.Module):
    def __init__(
            self,
            time_window: int,
            continuous_count: int,
            playmaker_count: int,
            playmaker_vocab_size: int,
            embedding_dim: int,
            output_dim: int):

        super().__init__()

        hidden_dim = 2000

        self.embedding = nn.Embedding(playmaker_vocab_size, embedding_dim)
        self.embedded_size = playmaker_count * embedding_dim * time_window

        self.continuous_projection = nn.Sequential(
                nn.Linear(continuous_count * time_window, hidden_dim),
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


if __name__ == "__main__":

    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    device = "cpu"
    print("Using device:", device)

    time_window = 200

    continuous_count = 35
    playmaker_count = 6
    playmaker_vocab_size = 40
    embedding_dim = 4
    output_dim = 10

    dataset = BossFightDataset("mossbone_mother.pt", time_window=time_window)
    dataloader = DataLoader(dataset, batch_size=16, shuffle=True)

    net = BossNet(time_window,
                  continuous_count,
                  playmaker_count,
                  playmaker_vocab_size,
                  embedding_dim,
                  output_dim).to(device)

    optimizer = optim.Adam(net.parameters(), lr=1e-3)
    criterion = nn.MSELoss()

    for epoch in range(15):

        total_loss = 0
        n = 0

        for continuous_batch, playmaker_batch, target_batch in dataloader:

            continuous_batch = continuous_batch.to(device, dtype=torch.float32)
            playmaker_batch = playmaker_batch.to(device, dtype=torch.long)
            target_batch = target_batch.to(device, dtype=torch.float32)

            optimizer.zero_grad()
            output_batch = net(continuous_batch, playmaker_batch)
            loss = criterion(output_batch, target_batch)
            loss.backward()
            optimizer.step()

            total_loss += loss.item()
            n += 1

        print(f"Epoch: {epoch} | Avg Loss: {total_loss/n}")
