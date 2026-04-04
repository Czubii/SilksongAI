import inspect

from numpy.f2py.auxfuncs import throw_error

from ai.models import model_registry, BaseBossNet


class ModelFactory:
    @staticmethod
    def construct(architecture_name, base_dims, **kwargs) -> BaseBossNet:
        if not architecture_name in model_registry.keys():
            raise Exception("Unknown model type")


        ModelFactory.validate_parameters(architecture_name, **kwargs)
        model_cls = model_registry[architecture_name]
        return model_cls(base_dimensions=base_dims, **kwargs)

    @staticmethod
    def validate_parameters(architecture_name, **kwargs) -> None:
        model_cls = model_registry[architecture_name]
        sig = inspect.signature(model_cls.__init__)

        required_params = []

        for name, param in sig.parameters.items():
            if name in ("self", "base_dimensions"):
                continue

            if param.default != inspect.Parameter.empty:
                required_params.append(name)

        missing = [p for p in required_params if p not in kwargs]

        allowed = {
            name for name in sig.parameters
            if name not in ("self", "base_dimensions")
        }

        unknown = [k for k in kwargs if k not in allowed]

        if missing:
            raise ValueError(f"Missing parameters for {architecture_name}: {', '.join(missing)}")

        if unknown:
            raise Exception(f"Unknown parameters for {architecture_name}: {unknown}")