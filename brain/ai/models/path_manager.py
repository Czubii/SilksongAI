import os
from pathlib import Path

from shared import load_config

config = load_config()

def generate_artifact_root_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(config.model_artifact_dir, target_boss_name, artifact_name))

def generate_artifact_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(generate_artifact_root_path(target_boss_name, artifact_name), "artifact.pt"))

def generate_artifact_dataset_folder_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(generate_artifact_root_path(target_boss_name, artifact_name), "dataset"))

def generate_artifact_test_dataset_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(generate_artifact_root_path(target_boss_name, artifact_name), "dataset", "testing_data.pt"))

def generate_artifact_training_dataset_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(generate_artifact_root_path(target_boss_name, artifact_name), "dataset", "training_data.pt"))
