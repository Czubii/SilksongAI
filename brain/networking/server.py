import asyncio
import traceback
import uuid
import msgpack

from networking.client_registry import client_registry
from networking.handler_registry import handler_registry
from networking.client_services import ClientConnection, ClientServices

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
    client_ID = uuid.uuid4()
    client = ClientConnection(writer)
    client_registry[client_ID] = client
    services = ClientServices(client)

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

            if req_type in handler_registry:
                try:
                    result = await handler_registry[req_type](payload, services)
                    response["payload"] = msgpack.packb(result, use_bin_type=True)
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
        services.on_disconnected()
        client_registry.pop(client_ID)
        await writer.wait_closed()

async def broadcast_event(event_type: str, payload: dict = None):
    payload_bytes = msgpack.packb(payload, use_bin_type=True)
    msg = {"kind": "event", "type": event_type, "payload": payload_bytes}
    for client_ID, client in client_registry.copy().items():
        try:
            data = msgpack.packb(msg, use_bin_type=True)
            client.writer.write(len(data).to_bytes(4, byteorder='big') + data)
            await client.writer.drain()
        except Exception:
            client_registry.pop(client_ID)


async def main():
    server = await asyncio.start_server(handle_client, '127.0.0.1', 5000)
    print("Server listening on 127.0.0.1:5000")

    async with server:
        await server.serve_forever()

if __name__ == "__main__":
    asyncio.run(main())

