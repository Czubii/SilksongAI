import asyncio
import traceback
import msgpack
from networking.client_manager import client_manager, ClientConnection
from networking.handler_registry import handler_registry


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
    client = ClientConnection(writer)
    client_manager.register_client(client)
    addr = writer.get_extra_info("peername")
    print(f"Client connected: {addr}")
    try:
        while True:
            request = await read_msg(reader)

            request_id = request.get("request_ID")
            req_type = request.get("type")
            payload = request.get("payload")

            response = {"kind": "response", "request_ID": request_id, "success": True, "log": "Success!", "payload": None}

            if req_type in handler_registry:
                handler = handler_registry[req_type]

                if handler.host_only and not client_manager.is_host_client(client):
                    response["success"] = False
                    response["log"] = f"Request \"{req_type}\" requires host privileges."
                    print(f"Request \"{req_type}\" requires host privileges.")
                else:
                    try:
                        result = await handler.func(payload)
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

            await write_msg(writer, response)

    except (asyncio.IncompleteReadError, ConnectionResetError):
        print(f"Client disconnected: {addr}")
    finally:
        writer.close()
        client_manager.remove_client(client)
        await writer.wait_closed()




async def main():
    server = await asyncio.start_server(handle_client, '0.0.0.0', 5000)
    print("Server listening on 127.0.0.1:5000")

    async with server:
        await server.serve_forever()

if __name__ == "__main__":
    asyncio.run(main())

