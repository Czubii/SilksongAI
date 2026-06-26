import importlib
import pkgutil

for _, module_name, is_pkg in pkgutil.iter_modules(__path__):
    if module_name not in {"registry", "base"}:
        importlib.import_module(f"{__name__}.{module_name}")