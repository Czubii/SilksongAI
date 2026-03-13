import asyncio
import msgpack
import numpy as np
import torch
from sympy.codegen.ast import bool_
from torchvision.models import list_models

import ai.models.registry
from ai.models import get_available_artifacts
from client_session import AIClientSession

handlers = {}

def register_handler(name):
    def decorator(func):
        handlers[name] = func
        return func
    return decorator

@register_handler("get_models")
async def get_models(payload, session: AIClientSession):
    output = {
        "selected_model": session.get_selected_model(),
        "models": get_available_artifacts()
    }
    return output

@register_handler("select_model")
async def select_model(payload, session: AIClientSession):
    session.select_model(payload["boss_name"], payload["model_name"])

    output = {
        "selected_model": session.get_selected_model(),
        "models": get_available_artifacts()
    }
    return output

@register_handler("predict_inputs")
async def predict_inputs(payload, session: AIClientSession):
    return session.predict_inputs(payload)

@register_handler("get_architectures")
async def get_architectures(payload, session: AIClientSession):
    payload = {}
    for name, architecture_cls in ai.models.registry.model_registry.items():
        payload[name] = architecture_cls.get_additional_param_definitions()
    return {"architectures": payload}


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
    session = AIClientSession()
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
                    print(f"Error: {e}")
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

