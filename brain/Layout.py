from dataclasses import dataclass
from typing import List

###------------------------------------------------------------------------------------------###
#
#   If you plan to add or remove some data in plugin's recorder remember to add them here. Also, the order must match 100%
#   For string variables (like playmakers) it's not that simple as they must be indexed before we can pass them to neural network
#
###------------------------------------------------------------------------------------------###

# The current version of the data structure this script supports
SUPPORTED_FORMAT_VERSION: int = 1

@dataclass
class PlayMakerData:
    name: str
    state_name: str

    @classmethod
    def from_raw(cls, raw):
        return cls(*raw)

@dataclass
class FrameEnemyData:
    pos_x: float
    pos_y: float
    facing: int
    vel_x: float
    vel_y: float
    hp: int
    playmakers: List[PlayMakerData]

    @classmethod
    def from_raw(cls, raw):
        return cls(
            *raw[:6],
            playmakers=[PlayMakerData.from_raw(pm) for pm in raw[6]]
        )

@dataclass
class FrameHeroData:
    pos_x: float
    pos_y: float
    facing: int
    vel_x: float
    vel_y: float
    hp: int
    silk: int
    can_jump: bool
    can_double_jump: bool
    can_attack: bool
    can_sprint: bool
    can_bind: bool
    can_cast: bool
    can_nail_art: bool
    can_try_harpoon: bool
    can_input: bool
    can_back_dash: bool

    @classmethod
    def from_raw(cls, raw):
        return cls(*raw)

@dataclass
class FrameUserInputs:
    jump: bool
    left: float
    right: float
    up: float
    down: float
    attack: bool
    heal: bool
    skill: bool
    dash: bool
    harpoon: bool

    @classmethod
    def from_raw(cls, raw):
        return cls(*raw)

@dataclass
class FrameData:
    boss: FrameEnemyData
    enemies: List[FrameEnemyData]
    hero: FrameHeroData
    inputs: FrameUserInputs

    @classmethod
    def from_raw(cls, raw):
        return cls(
            boss=FrameEnemyData.from_raw(raw[0]),
            enemies=[FrameEnemyData.from_raw(e) for e in raw[1]],
            hero=FrameHeroData.from_raw(raw[2]),
            inputs=FrameUserInputs.from_raw(raw[3])
        )