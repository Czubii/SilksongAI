import asyncio
import msgpack


handlers = {}

def register_handler(name):
    def decorator(func):
        handlers[name] = func
        return func

    return decorator

@register_handler("get_models")
async def handle_ping(payload):
    return ["cipa", "cyce", "jak", "donice"]

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
    addr = writer.get_extra_info("peername")
    print(f"Client connected: {addr}")
    try:
        while True:
            request = await read_msg(reader)

            request_id = request.get("RequestId")
            req_type = request.get("Type")
            payload = request.get("Payload")

            response = {"RequestId": request_id, "Success": True, "Payload": None}

            if req_type in handlers:
                try:
                    result = await handlers[req_type](payload)
                    print(result)
                    response["Payload"] = result
                except Exception as e:
                    response["Success"] = False
                    response["Payload"] = str(e)
            else:
                response["Success"] = False
                response["Payload"] = f"Unknown request type: {req_type}"
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