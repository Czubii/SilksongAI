from dataclasses import dataclass, fields
from typing import TypeVar, Type, List, Any, Iterable

T = TypeVar("T", bound="BaseLayout")

continuous_types = {int, float}

@dataclass
class BaseLayout:

    @staticmethod
    def _is_base_layout_subclass(f) -> bool:
        return isinstance(f.type, type) and issubclass(f.type, BaseLayout)

    @staticmethod
    def _is_list(f) -> bool:
        return hasattr(f.type, "__origin__") and f.type.__origin__ is list

    def __post_init__(self):
        self._count_cache: dict[type, int] = {}
        self._building_indexes = []

        self._type_indexes: dict[type, list[Any]] = {}
        self._field_names = [f.name for f in fields(self)]

        self.base_layout_indexes = tuple(i for i, f in enumerate(fields(self))
                                       if BaseLayout._is_base_layout_subclass(f))
        self.list_indexes = tuple(i for i, f in enumerate(fields(self))
                                    if BaseLayout._is_list(f))
        self.primitive_indexes = tuple(i for i, f in enumerate(fields(self))
                                       if not BaseLayout._is_base_layout_subclass(f)
                                       and not BaseLayout._is_list(f))



    def update(self, data: List[Any]) -> None:
        """
        Updates the data with new raw indexed list. Uses cached indexes and therefore is a lot faster. Of course the
        dimensions of input data must be the same
        """
        for idx in self.base_layout_indexes:
            getattr(self, self._field_names[idx]).update(data[idx])

        for idx in self.list_indexes:
            field_name = self._field_names[idx]
            for inner_idx, entry in enumerate(getattr(self, field_name)):

                if issubclass(type(entry), BaseLayout):
                    entry.update(data[idx][inner_idx])
                else:
                    getattr(self, field_name)[inner_idx] = data[idx][inner_idx]

        for idx in self.primitive_indexes:
            setattr(self, self._field_names[idx], data[idx])

    @classmethod
    def from_indexed_list(cls: Type[T], data: List[Any]) -> T:
        """
        Only use this the first time you need to initialize the object. Then use the update() method as it is a lot faster
        """
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
        total = 0
        for t in continuous_types:
            total += self.type_count(t)
        return total

    def boolean_count(self) -> int:
        return self.type_count(bool)

    def type_count(self, target_type: type[Any]) -> int:
        if target_type in self._count_cache:
            return self._count_cache[target_type]
        count = self._count_recursive(self, target_type)
        self._count_cache[target_type] = count
        return count

    def _count_recursive(self, data: Any, target_type: type[Any]) -> int:
        total = 0
        if isinstance(data, (list, tuple)):
            for item in data:
                total += self._count_recursive(item, target_type)

        elif isinstance(data, BaseLayout):
            for field in fields(data):
                val = getattr(data, field.name)
                total += self._count_recursive(val, target_type)

        if type(data) == target_type:
            return 1

        return total

    # def get_all_continuous(self) -> List[Any]:
    #     return self._get_of_type_recursive(self, continuous_types)
    #
    # def get_all_booleans(self) -> List[Any]:
    #     return self._get_of_type_recursive(self, bool)
    #
    # def get_all_of_type(self, target_type: type(Any)) -> List[Any]:
    #     return self._get_of_type_recursive(self, target_type)
    #
    # def _get_of_type_recursive(self, data: Any, counted_type: type) -> List[Any]:
    #     output = []
    #
    #     if isinstance(data, (list, tuple)):
    #         for item in data:
    #             output.extend(self._get_of_type_recursive(item, counted_type))
    #
    #     elif isinstance(data, BaseLayout):
    #         for field in fields(data):
    #             val = getattr(data, field.name)
    #             output.extend(self._get_of_type_recursive(val, counted_type))
    #
    #     if type(data) == counted_type:
    #         output.append(data)
    #
    #     return output

    def get_all_continuous(self) -> List[Any]:
        return self.get_all_of_types(continuous_types)

    def get_all_booleans(self) -> List[Any]:
        return self.get_all_of_type(bool)

    def get_all_of_types(self, target_types: Iterable[type]) -> List[Any]:
        output = []
        for t in target_types:
            output.extend(self.get_all_of_type(t))
        return output

    def get_all_of_type(self, target_type: type[Any]) -> List[Any]:
        if target_type in self._type_indexes:
            indexes = self._type_indexes[target_type]
            return self._get_all_of_type_fast_recursive(self, indexes)
        else:
            self._type_indexes[target_type] = self._get_type_indexes(self, target_type)
            indexes = self._type_indexes[target_type]
            return self._get_all_of_type_fast_recursive(self, indexes)

    def _get_all_of_type_fast_recursive(self, data, indexes: list[tuple[int]]) -> List[Any]:
        output = []
        for (idx, inner) in indexes:
            if inner == [0]:
                output.append(data[idx])
            else:
                output.extend(self._get_all_of_type_fast_recursive(data[idx], inner))
        return output

    def _get_type_indexes(self, data: Any, t: type) -> list[int]:
        output = []

        if isinstance(data, (list, tuple)):
            for i, item in enumerate(data):
                indexes = self._get_type_indexes(item, t)
                if len(indexes) == 0: continue
                output.append((i,indexes))

        elif isinstance(data, BaseLayout):
            for i, field in enumerate(fields(data)):
                val = getattr(data, field.name)
                indexes = self._get_type_indexes(val, t)
                if len(indexes) == 0: continue
                output.append((i,indexes))

        if type(data) == t:
            output = [0]

        return output

    def __getitem__(self, idx):
        return getattr(self, self._field_names[idx])

    def __len__(self):
        return len(self._field_names)