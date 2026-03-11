import tomllib
from pathlib import Path

CONFIG_PATH = Path(__file__).parent.parent / "config.toml.user"

class Config:
    recording_dir: Path
    model_artifact_dir: Path


def load_config() -> Config:
    if not CONFIG_PATH.exists():
        raise RuntimeError(f"{CONFIG_PATH} not found")

    with open(CONFIG_PATH, "rb") as f:
        data = tomllib.load(f)

    cfg = Config()
    cfg.recording_dir = Path(data["paths"]["recording_dir"])
    cfg.model_artifact_dir = Path(data["paths"]["model_artifact_dir"])

    return cfg