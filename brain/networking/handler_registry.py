handler_registry = {}

def register_handler(name):
    def decorator(func):
        handler_registry[name] = func
        return func
    return decorator