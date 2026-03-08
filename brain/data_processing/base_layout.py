from dataclasses import dataclass, fields
from typing import TypeVar, Type, List, Any

T = TypeVar("T", bound="BaseLayout")

continuous_types = [int, float]

@dataclass
class BaseLayout: #TODO: add a way to bake the indexes to avoid total search every time we want to find something which may get too slow for live inference
    @classmethod
    def from_indexed_list(cls: Type[T], data: List[Any]) -> T:

        if data is None:
            return None

        init_args = {}
        for i, field in enumerate(fields(cls)):
            #print(f"{field.type}: {data[i]}") useful for debugging

            raw_value = data[i]

            # case 1: nested BaseLayout
            if isinstance(field.type, type) and issubclass(field.type, BaseLayout):
                init_args[field.name] = field.type.from_indexed_list(raw_value)

            # case 2: Lists
            elif hasattr(field.type, "__origin__") and field.type.__origin__ is list:
                inner_type = field.type.__args__[0]

                #print(inner_type) useful for debugging

                if issubclass(inner_type, BaseLayout):
                    init_args[field.name] = [inner_type.from_indexed_list(item) for item in raw_value]
                else:
                    init_args[field.name] = raw_value

            # case 3: primitives
            else:
                init_args[field.name] = raw_value

        return cls(**init_args)

    @classmethod
    def get_field_count(cls: Type[T]) -> int:
        return len(fields(cls))

    def continuous_count(self) -> int:
        return self.type_count(continuous_types)

    def boolean_count(self) -> int:
        return self.type_count([bool])

    def type_count(self, target_type: List[type(Any)]) -> int:
        return self._count_recursive(self, target_type)

    def _count_recursive(self, data: Any, counted_types: List[Any]) -> int:
        total = 0
        if isinstance(data, (list, tuple)):
            for item in data:
                total += self._count_recursive(item, counted_types)

        elif isinstance(data, BaseLayout):
            for field in fields(data):
                val = getattr(data, field.name)
                total += self._count_recursive(val, counted_types)

        if type(data) in counted_types:
            return 1

        return total

    def get_all_continuous(self) -> List[Any]:
        return self._get_of_type_recursive(self, continuous_types)

    def get_all_booleans(self) -> List[Any]:
        return self._get_of_type_recursive(self, [bool])

    def get_all_of_type(self, target_type: List[type(Any)]) -> List[Any]:
        return self._get_of_type_recursive(self, target_type)

    def _get_of_type_recursive(self, data: Any, counted_types: List[Any]) -> List[Any]:
        output = []

        if isinstance(data, (list, tuple)):
            for item in data:
                output.extend(self._get_of_type_recursive(item, counted_types))

        elif isinstance(data, BaseLayout):
            for field in fields(data):
                val = getattr(data, field.name)
                output.extend(self._get_of_type_recursive(val, counted_types))

        if type(data) in counted_types:
            output.append(data)

        return output