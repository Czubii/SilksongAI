import asyncio
import msgpack
import numpy as np
import torch
from sympy.codegen.ast import bool_
from torchvision.models import list_models

from Networks import BossModelArtifact, BossModelFactory
from Preprocessing import LivePreprocessor
from RecordingLayout import Layout
from model_manager import get_all_models, model_exists, get_model_path

handlers = {}

class ClientSession: #TODO: add permanence in case of disconnect
    def __init__(self):
        self._selected_boss_name = ""
        self._selected_model_name = ""
        self._artifact: BossModelArtifact = None
        self._live_preprocessor: LivePreprocessor = None


        self.cont_buffer = None
        self.playmaker_buffer = None
        self.buffer_index = 0
        self.buffer_filled = False
        self.time_window = 1

    def select_model(self, boss_name: str, model_name: str, load_artifact: bool = True):

        self._artifact: BossModelArtifact = None
        self._live_preprocessor: LivePreprocessor = None

        if not model_exists(boss_name, model_name):
            self._selected_boss_name = ""
            self._selected_model_name = ""
            return

        self._artifact = BossModelFactory.load(get_model_path(boss_name, model_name)) #TODO add separate button for loading or load when starting session

        self._live_preprocessor = LivePreprocessor(self._artifact)

        config = self._artifact.model.config
        self.time_window = config["time_window"]
        cont_feature_size = config["cont_dim"]
        playmaker_feature_size = config["playmaker_dim"]

        self.cont_buffer = torch.zeros((self.time_window, cont_feature_size), dtype=torch.float32)
        self.playmaker_buffer = torch.zeros((self.time_window, playmaker_feature_size), dtype=torch.int32)

        self._selected_model_name = model_name
        self._selected_boss_name = boss_name

        self.buffer_filled = False

    def get_selected_model(self):
        return [self._selected_boss_name, self._selected_model_name]

    def predict_inputs(self, frame_data):
        if self._artifact is None:
            raise ValueError("Artifact is not loaded")

        if self._live_preprocessor is None:
            raise ValueError("LivePreprocessor is not loaded")

        try:
            processed_cont, processed_playmaker = self._live_preprocessor.process_frame(frame_data)
        except Exception as e:
            raise Exception(f"Got exception while processing frame: {e}")

        self.cont_buffer[self.buffer_index] = torch.from_numpy(processed_cont)
        self.playmaker_buffer[self.buffer_index] = torch.from_numpy(processed_playmaker)

        self.buffer_index += 1

        if self.buffer_index == self.time_window:
            self.buffer_index = 0
            self.buffer_filled = True

        if self.buffer_filled:
            cont_window = torch.roll(self.cont_buffer, -self.buffer_index, dims=0)
            playmaker_window = torch.roll(self.playmaker_buffer, -self.buffer_index, dims=0)
        else:
            cont_window = self.cont_buffer[:self.buffer_index]
            playmaker_window = self.playmaker_buffer[:self.buffer_index]

            return None

        try:
            cont_window_batched = cont_window.unsqueeze(0)
            playmaker_window_batched = playmaker_window.unsqueeze(0)

            with torch.no_grad():
                output = self._artifact.model(cont_window_batched, playmaker_window_batched)
        except Exception as e:
            raise Exception(
                f"Got exception while predicting inputs: {e} | "
                f"cont_shape={cont_window.shape} "
                f"playmaker_shape={playmaker_window.shape}"
            ) from e

        output = output.squeeze(0)  # remove batch dimension

        float_outputs = output[:Layout.Inputs.float_values]
        bool_logits = output[Layout.Inputs.float_values:]

        bool_probs = torch.sigmoid(bool_logits)
        bool_values = (bool_probs > 0.15).tolist()

        float_values = float_outputs.tolist()

        print(float_values)

        payload = float_values + bool_values

        return payload



def register_handler(name):
    def decorator(func):
        handlers[name] = func
        return func
    return decorator

@register_handler("get_models")
async def get_models(payload, session: ClientSession):
    output = {
        "selected_model": session.get_selected_model(),
        "models": get_all_models()
    }
    return output

@register_handler("select_model")
async def select_model(payload, session: ClientSession):

    session.select_model(payload["boss_name"], payload["model_name"])

    output = {
        "selected_model": session.get_selected_model(),
        "models": get_all_models()
    }
    return output

@register_handler("predict_inputs")
async def predict_inputs(payload, session: ClientSession):
    return session.predict_inputs(payload)


async def read_msg(reader):
    length_bytes = await reader.readexactly(4)
    length = int.from_bytes(length_bytes, byteorder='big')
    data = await reader.readexactly(length)

    out = msgpack.unpackb(data, raw=False)

    return out

async def write_msg(writer, msg):
    data = msgpack.packb(msg, use_bin_type=True)
    writer.write(len(data).to_bytes(4, byteorder='big') + data)
    await writer.drain()

async def handle_client(reader, writer):
    session = ClientSession()
    addr = writer.get_extra_info("peername")
    print(f"Client connected: {addr}")
    try:
        while True:
            request = await read_msg(reader)

            request_id = request.get("request_ID")
            req_type = request.get("type")
            payload = request.get("payload")

            response = {"request_ID": request_id, "success": True, "error_message": "", "payload": None}

            if req_type in handlers:
                try:
                    result = await handlers[req_type](payload, session)
                    #print(result)
                    response["payload"] = result
                except Exception as e:
                    response["success"] = False
                    response["error_message"] = str(e)
            else:
                response["success"] = False
                response["error_message"] = f"Unknown request type: {req_type}"
                print(f"Unknown request type: {req_type}")

            await write_msg(writer, response)

    except (asyncio.IncompleteReadError, ConnectionResetError):
        print(f"Client disconnected: {addr}")
    finally:
        writer.close()
        await writer.wait_closed()

async def main():
    server = await asyncio.start_server(handle_client, '127.0.0.1', 5000)
    print("Server listening on 127.0.0.1:5000")

    async with server:
        await server.serve_forever()

if __name__ == "__main__":
    asyncio.run(main())