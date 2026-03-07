import math
import os
import shutil
from collections import defaultdict
from operator import contains

from sympy.printing.pytorch import torch

from Networks import BossNet, BossModelArtifact
from Preprocessing import ProcessingPipeline
from RecordingLayout import Layout


def get_all_models() -> defaultdict[str, list[str]]:
    output = defaultdict(list[str])
    for (dirpath, dirnames, filenames) in os.walk("models"):
        if contains(filenames, "model.pt"):
            checkpoint = torch.load(os.path.join(dirpath, "model.pt"), map_location="cpu", weights_only=False)
            if checkpoint.get("format_version", 1) != Layout.SUPPORTED_FORMAT_VERSION:
                continue

            folders = dirpath.split("\\")
            output[folders[-2]].append(folders[-1])

    return output


def model_exists(target_boss_name: str, model_name: str) -> bool:
    models = get_all_models()
    return model_name in models[target_boss_name]

def model_dir_exists (target_boss_name: str, model_name: str) -> bool:
    return os.path.exists(os.path.join("models", target_boss_name, model_name))

def delete_model(target_boss_name: str, model_name: str):
    if model_dir_exists(target_boss_name, model_name):
        shutil.rmtree(os.path.join("models", target_boss_name, model_name))

def get_model_root_path(target_boss_name: str, model_name: str):
    return os.path.join("models", target_boss_name, model_name)

def get_model_path(target_boss_name: str, model_name: str):
    return os.path.join(get_model_root_path(target_boss_name, model_name), "model.pt")

def get_model_dataset_path(target_boss_name: str, model_name: str):
    return os.path.join(get_model_root_path(target_boss_name, model_name), "dataset")

def new_model(target_boss_name: str,
              model_name: str,
              raw_recording_dir_path: str,
              overwrite_if_exists: bool = False,
              num_testing = 1,
              time_window = 4,
              embedding_dim: int = None,
              hidden_dim = 120):
    """
    :param target_boss_name:
    :param model_name: name of newly created model
    :param raw_recording_dir_path: path to raw recordings.
    :param overwrite_if_exists: whether to overwrite existing model and its processed dataset or not.
    :param num_testing: number of recordings that should be used for testing
    :param time_window: time window of the model in frames
    :param embedding_dim: dimension of the embedding layer
    :return:
    """
    if model_dir_exists(target_boss_name, model_name):
        if not overwrite_if_exists:
            raise NameError("Model or directory with such name already exists")
        else:
            delete_model(target_boss_name, model_name)

    model_path = get_model_root_path(target_boss_name, model_name)
    dataset_path = get_model_dataset_path(target_boss_name, model_name)
    os.makedirs(model_path)
    os.makedirs(dataset_path)

    try:
        pp = ProcessingPipeline(data_path=raw_recording_dir_path,
                                    target_boss=target_boss_name)

        pp.process_and_save(output_dir=dataset_path, num_testing=num_testing)

        vocab = pp.get_vocabulary().get_raw()

        dims = pp.get_dims()

        if embedding_dim is None:
            embedding_dim = int(pow(dims["vocab_dim"], 0.25))

        network = BossNet(time_window,
                          dims["cont_dim"],
                          dims["playmaker_dim"],
                          dims["vocab_dim"],
                          embedding_dim,
                          hidden_dim,
                          Layout.Inputs.num_elements)

        metadata = {
            "epochs": 0,
            "loss": math.nan,
            "transformations": {
                "cont_mean": pp.get_cont_mean(),
                "cont_std": pp.get_cont_std()
            }
        }

        model_artifact = BossModelArtifact(network, target_boss_name, vocab, metadata)

        model_artifact.save(get_model_path(target_boss_name, model_name))

    except Exception as e:
        delete_model(target_boss_name, model_name)
        raise e


if __name__ == "__main__":
    new_model(
        "Lace Boss1",
        "safe",
        "../recordings/Lace Boss1",
        True,
                num_testing=1,
                hidden_dim=80,
                time_window=20,
                embedding_dim=5)