import asyncio
import msgpack
from torchvision.models import list_models

from Networks import BossModelArtifact, BossModelFactory
from model_manager import get_all_models, model_exists, get_model_path

handlers = {}

class ClientSession: #TODO: add permanence in case of disconnect
    def __init__(self):
        self._selected_boss_name = ""
        self._selected_model_name = ""
        self._artifact: BossModelArtifact = None

    def select_model(self, boss_name: str, model_name: str, load_artifact: bool = True):
        if not model_exists(boss_name, model_name):
            self._selected_boss_name = ""
            self._selected_model_name = ""
            return

        self._artifact = BossModelFactory.load(get_model_path(boss_name, model_name)) #TODO add separate button for loading or load when starting session

        self._selected_model_name = model_name
        self._selected_boss_name = boss_name

    def get_selected_model(self):
        return [self._selected_boss_name, self._selected_model_name]

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
                    print(result)
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