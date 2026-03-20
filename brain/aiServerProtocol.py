import asyncio
import traceback
from logging import exception

import msgpack
import numpy as np
import torch
from sympy.codegen.ast import bool_
from torchvision.models import list_models

import ai.models.registry
from ai.models import get_available_artifacts, prepare_dataset_and_artifact
from client_session import AIClientSession
from data_processing import RecordingFilters, ChoosePercentBest

handlers = {}

def register_handler(name):
    def decorator(func):
        handlers[name] = func
        return func
    return decorator

@register_handler("get_artifacts")
async def get_models(payload: dict, session: AIClientSession):
    output = {
        "artifacts": get_available_artifacts()
    }
    return output

@register_handler("select_model")
async def select_model(payload: dict, session: AIClientSession):
    session.select_model(payload["boss_name"], payload["model_name"])

    output = {
        "selected_model": session.get_selected_model(),
        "models": get_available_artifacts()
    }
    return output

@register_handler("predict_inputs")
async def predict_inputs(payload: dict, session: AIClientSession):
    return session.predict_inputs(payload)

@register_handler("get_architectures")
async def get_architectures(payload: dict, session: AIClientSession):
    payload = {}
    for name, architecture_cls in ai.models.registry.model_registry.items():
        payload[name] = architecture_cls.get_additional_param_definitions()
    return {"architectures": payload}


def deserialize_additional_params(params: dict):
    deserialized = {}
    for param in params:
        if param["type"] == "int":
            deserialized[param["name"]] = int(param["value"])
        else:
            raise Exception(f"Unsupported type {param['type']}")

    return deserialized



@register_handler("new_model")
async def new_model(payload: dict, session: AIClientSession):
    quality_filter = ChoosePercentBest(payload["percent_best"])
    filters = RecordingFilters(payload["require_success"],
                               payload["delta"],
                               payload["player_name"],
                               quality_filter)

    network_params = deserialize_additional_params(payload["params"])

    print(network_params)
    prepare_dataset_and_artifact(payload["target_boss"],
                                 payload["name"],
                                 filters,
                                 payload["overwrite"],
                                 0.2,
                                 **network_params)

    await broadcast_event("new_artifact")

clients = set()

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
    clients.add(writer)
    session = AIClientSession()
    addr = writer.get_extra_info("peername")
    print(f"Client connected: {addr}")
    try:
        while True:
            request = await read_msg(reader)

            request_id = request.get("request_ID")
            req_type = request.get("type")
            payload = request.get("payload")

            print(request)

            response = {"kind": "response", "request_ID": request_id, "success": True, "log": "Success!", "payload": None}

            if req_type in handlers:
                try:
                    result = await handlers[req_type](payload, session)
                    #print(result)
                    response["payload"] = result
                except Exception as e:
                    response["success"] = False

                    tb = traceback.format_exc()
                    response["log"] = f"{e} \n {str(tb)}"
                    print(f"Error: {tb}")
            else:
                response["success"] = False
                response["log"] = f"Unknown request type: {req_type}"
                print(f"Unknown request type: {req_type}")


            print(response)
            await write_msg(writer, response)

    except (asyncio.IncompleteReadError, ConnectionResetError):
        print(f"Client disconnected: {addr}")
    finally:
        writer.close()
        clients.remove(writer)
        await writer.wait_closed()

async def broadcast_event(type: str):
    msg = {"kind": "event", "type": type}
    for client in clients.copy():
        try:
            data = msgpack.packb(msg, use_bin_type=True)
            client.write(len(data).to_bytes(4, byteorder='big') + data)
            await client.drain()
        except Exception:
            clients.remove(client)


async def main():
    server = await asyncio.start_server(handle_client, '127.0.0.1', 5000)
    print("Server listening on 127.0.0.1:5000")

    async with server:
        await server.serve_forever()

if __name__ == "__main__":
    asyncio.run(main())

