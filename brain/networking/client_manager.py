import traceback
from typing import Optional
import msgpack

class ClientConnection:
    def __init__(self, writer):
        self.writer = writer

    async def send_event(self, event_type: str, payload: dict = None):
        payload_bytes = msgpack.packb(payload, use_bin_type=True)
        msg = {"kind": "event", "type": event_type, "payload": payload_bytes}
        try:
            data = msgpack.packb(msg, use_bin_type=True)
            self.writer.write(len(data).to_bytes(4, byteorder='big') + data)
            await self.writer.drain()
        except Exception:
            print(f"Error: {traceback.format_exc()}")

class ClientManager:
    def __init__(self):
        self.worker_clients: set[ClientConnection] = set([])
        self.host_client: Optional[ClientConnection] = None

    def register_client(self, client: ClientConnection) -> None:
        self.worker_clients.add(client)
        if self.host_client is None:
            self.host_client = client

    def remove_client(self, client: ClientConnection) -> None:
        self.worker_clients.remove(client)
        if self.is_host_client(client):
            self.host_client = None

    def is_host_client(self, client: ClientConnection) -> bool:
        return self.host_client is client

    async def broadcast_worker_event(self, event_type: str, payload: dict = None):
        payload_bytes = msgpack.packb(payload, use_bin_type=True)
        msg = {"kind": "event", "type": event_type, "payload": payload_bytes}
        for client in self.worker_clients.copy():
            try:
                data = msgpack.packb(msg, use_bin_type=True)
                client.writer.write(len(data).to_bytes(4, byteorder='big') + data)
                await client.writer.drain()
            except Exception:
                self.worker_clients.remove(client)

    async def send_host_event(self, event_type: str, payload: dict = None):
        await self.host_client.send_event(event_type, payload)

client_manager = ClientManager()