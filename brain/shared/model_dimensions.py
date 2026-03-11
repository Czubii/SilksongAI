from dataclasses import dataclass

@dataclass
class ModelDimensions:
    input_continuous: int
    input_boolean: int
    input_named_state: int

    output_continuous: int
    output_boolean: int

    vocab_word_count: int

    @property
    def total_input(self) -> int:
        return self.input_continuous + self.input_boolean + self.input_named_state

    @property
    def total_output(self) -> int:
        return self.output_continuous + self.output_boolean