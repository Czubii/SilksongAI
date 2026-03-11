import torch
from torch import softmax

from ai.models import Artifact, artifact_exists, ArtifactFactory, get_artifact_path
from data_processing import LiveInferenceFrame


class AIClientSession:
    """
    Wraps the live inference process for single client
    """
    def __init__(self):
        self._selected_boss_name = ""
        self._selected_model_name = ""
        self._artifact: Artifact = None

        self._cont_buffer = torch.tensor([])
        self._bool_buffer = torch.tensor([])
        self._named_state_buffer = torch.tensor([])
        self._buffer_filled = False

    def select_model(self, boss_name: str, model_name: str, load_artifact: bool = True):

        self._artifact: Artifact = None

        if not artifact_exists(boss_name, model_name):
            self._selected_boss_name = ""
            self._selected_model_name = ""
            return
        print(f"Selected model: {self._selected_boss_name}-{self._selected_model_name}")
        self._artifact = ArtifactFactory.from_file(get_artifact_path(boss_name, model_name))  # TODO add separate button for loading or load when starting session

        self._selected_model_name = model_name
        self._selected_boss_name = boss_name

        print(f"Selected model: {self._selected_boss_name}-{self._selected_model_name}")

        model = self._artifact.model
        self._cont_buffer = torch.zeros([model.time_window, model.base_dimensions.input_continuous], dtype=torch.float32)
        self._bool_buffer = torch.zeros([model.time_window, model.base_dimensions.input_boolean], dtype=torch.float32)
        self._named_state_buffer = torch.zeros([model.time_window, model.base_dimensions.input_named_state], dtype=torch.long)
        self._buffer_filled = False


    def get_selected_model(self):
        return [self._selected_boss_name, self._selected_model_name]

    def _update_buffer(self, cont_tensor, bool_tensor, named_state_tensor):
        # the higher the index the newer the frame
        self._cont_buffer = torch.roll(self._cont_buffer, -1, 0)
        self._bool_buffer = torch.roll(self._bool_buffer, -1, 0)
        self._named_state_buffer = torch.roll(self._named_state_buffer, -1, 0)

        idx = self._artifact.model.time_window-1
        self._cont_buffer[idx] = cont_tensor
        self._bool_buffer[idx] = bool_tensor
        self._named_state_buffer[idx] = named_state_tensor
        self._buffer_filled = True

        print(self._named_state_buffer)

    def predict_inputs(self, frame_raw):
        if self._artifact is None:
            raise ValueError("Artifact is not loaded")

        frame = LiveInferenceFrame.from_indexed_list(frame_raw)

        try:
            continuous, boolean, named_state = self._artifact.frame_processor.process_frame(frame.FrameData)
        except Exception as e:
            raise Exception(f"Got exception while processing frame: {e}")

        cont_tensor = torch.tensor(continuous, dtype=torch.float32)
        bool_tensor = torch.tensor(boolean, dtype=torch.float32)
        named_state_tensor = torch.tensor(named_state, dtype=torch.long)
        print(named_state_tensor)
        self._update_buffer(cont_tensor, bool_tensor, named_state_tensor)

        if not self._buffer_filled:
            return []

        cont_batched = self._cont_buffer.unsqueeze(0)
        bool_batched = self._bool_buffer.unsqueeze(0)
        named_state_batched = self._named_state_buffer.unsqueeze(0)
        output = self._artifact.model(cont_batched, bool_batched, named_state_batched)

        prediction_continuous_batch = output["output_continuous"]
        prediction_boolean_batch = output["output_boolean"]

        prediction_continuous_batch = prediction_continuous_batch.squeeze(0)
        prediction_boolean_batch = prediction_boolean_batch.squeeze(0)

        bool_probabilities = torch.sigmoid(prediction_boolean_batch)
        bool_values = (bool_probabilities > 0.05).tolist()
        cont_values = prediction_continuous_batch.tolist()
        payload = cont_values + bool_values
        return payload