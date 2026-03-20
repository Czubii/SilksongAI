import os
import shutil
from collections import defaultdict
from dataclasses import dataclass
from operator import contains
from pathlib import Path
import torch

from ai.models.model_artifact import ArtifactFactory
from shared import load_config
from data_processing import SUPPORTED_LAYOUT_VERSION, RecordingQualityFilter, ChooseNBest, ChoosePercentBest

from data_processing import RecordingProcessor
from data_processing import RecordingFilters

config = load_config()

@dataclass
class ArtifactPaths:
    artifact: Path
    testing_dataset: Path
    training_dataset: Path

def get_available_artifacts() -> defaultdict[str, list[str]]:
    output = defaultdict(list[str])
    for (dirpath, _, filenames) in os.walk(config.model_artifact_dir):
        if contains(filenames, "artifact.pt"):
            checkpoint = torch.load(os.path.join(dirpath, "artifact.pt"), map_location="cpu", weights_only=False)
            if checkpoint.get("layout_version", 1) != SUPPORTED_LAYOUT_VERSION:
                continue

            folders = dirpath.split("\\")
            output[folders[-2]].append(folders[-1])

    return output

def get_artifact_paths(target_boss_name: str, artifact_name: str) -> ArtifactPaths:
    if not artifact_exists(target_boss_name, artifact_name):
        raise ValueError(f"Artifact {target_boss_name}: {artifact_name} does not exist")
    return ArtifactPaths(
        artifact=get_artifact_path(target_boss_name, artifact_name),
        testing_dataset=get_artifact_test_dataset_path(target_boss_name, artifact_name),
        training_dataset=get_artifact_training_dataset_path(target_boss_name, artifact_name),
    )

def artifact_exists(target_boss_name: str, artifact_name: str) -> bool:
    artifacts = get_available_artifacts()
    return artifact_name in artifacts[target_boss_name]

def artifact_dir_exists (target_boss_name: str, artifact_name: str) -> bool:
    return os.path.exists(os.path.join(config.model_artifact_dir, target_boss_name, artifact_name))

def delete_artifact(target_boss_name: str, artifact_name: str):
    if artifact_dir_exists(target_boss_name, artifact_name):
        shutil.rmtree(os.path.join(config.model_artifact_dir, target_boss_name, artifact_name))

def get_artifact_root_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(config.model_artifact_dir, target_boss_name, artifact_name))

def get_artifact_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(get_artifact_root_path(target_boss_name, artifact_name), "artifact.pt"))

def get_artifact_dataset_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(get_artifact_root_path(target_boss_name, artifact_name), "dataset"))

def get_artifact_test_dataset_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(get_artifact_root_path(target_boss_name, artifact_name), "dataset", "testing_data.pt"))

def get_artifact_training_dataset_path(target_boss_name: str, artifact_name: str) -> Path:
    return Path(os.path.join(get_artifact_root_path(target_boss_name, artifact_name), "dataset", "training_data.pt"))

def prepare_dataset_and_artifact(target_boss_name: str,
                                 artifact_name: str,
                                 filters: RecordingFilters,
                                 overwrite = False,
                                 testing_percent = 0.2,
                                 architecture_name ="ExemplaryNet",
                                 **kwargs) -> None:

    output_root = get_artifact_root_path(target_boss_name, artifact_name)
    if output_root.exists() and not overwrite:
        raise ValueError(f"Artifact with name \"{artifact_name}\" for boss \"{target_boss_name}\" already exists. Set overwrite=True to overwrite.")
    elif output_root.exists():
        delete_artifact(target_boss_name, artifact_name)

    os.makedirs(output_root, exist_ok=True)

    recording_path = os.path.join(config.recording_dir, target_boss_name)
    if not os.path.exists(recording_path):
        raise ValueError(f"Recordings of {target_boss_name} are missing")

    pp = RecordingProcessor(recording_path, target_boss_name, filters)

    dataset_path = get_artifact_dataset_path(target_boss_name, artifact_name)
    os.mkdir(dataset_path)

    pp.process_and_save(dataset_path, testing_percent=testing_percent)

    artifact = ArtifactFactory.from_dataset(dataset_path, architecture_name, **kwargs)
    artifact.save(get_artifact_path(target_boss_name, artifact_name))

if __name__ == '__main__':
    filters = RecordingFilters()
    filters.quality_filter = ChoosePercentBest(1.0)
    prepare_dataset_and_artifact("Lace Boss1",
                                 "test1",
                                 filters,
                                 overwrite=True,
                                 architecture_name="ExemplaryNet",
                                 time_window=5,
                                 embedding_dim=4,
                                 hidden_dim=120,
                                 testing_percent=0.01)