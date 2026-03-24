import uuid

from networking.client_services import ClientConnection

client_registry: dict[uuid.UUID, ClientConnection] = {}