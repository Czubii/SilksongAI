from dataclasses import dataclass
from typing import List

from .base_layout import BaseLayout


###------------------------------------------------------------------------------------------###
#
#   If you plan to add or remove some data in plugin's recorder remember to add them here and always increment
#   SUPPORTED_LAYOUT_VERSION (also, the order of variables MUST MATCH 100%)
#   String variables, like enemy playmakers, MUST be stored inside NamedStatesContainer as vocabulary will be created only
#   for this class.
#
#   Supported Types: int, float, bool_input, NamedStatesContainer and lists containing those - any of those should automatically be converted and
#   standardized. Any variable with unsupported type will just get skipped.
#
#   Difference between continuous types (i.e. int-s and float-s) and the booleans is that the booleans won't be standardized
#   in processing step as they are well-behaved in the first place. It's also important to note that for the
#   behavioral learning step the boolean-type variables of the recording's user inputs get weights assigned.
#   Without it the network will have trouble learning to "use the buttons" so it's very important to implement everything
#   correctly here.
#
###------------------------------------------------------------------------------------------###

class LayoutNotSupported(Exception):
    pass

SUPPORTED_LAYOUT_VERSION: int = 16

@dataclass
class NamedState(BaseLayout):
    Name: str
    StateName: str

@dataclass
class NamedStatesContainer(BaseLayout):
    ParentName: str # for example enemy name
    NamedStates: List[NamedState] # for example list of enemy playmakers

@dataclass
class Hero(BaseLayout):
    PosX: float
    PosY: float
    RelPosX: float
    RelPosY: float
    HP: int
    Silk: int
    IsStunned: bool
    CanJump: bool
    CanDoubleJump: bool
    CanAttack: bool
    CanSprint: bool
    CanBind: bool
    CanCast: bool
    CanNailArt: bool
    CanTryHarpoon: bool
    CanBackDash: bool

    InvDistX: float
    InvDistY: float

@dataclass
class Enemy(BaseLayout):
    PosX: float
    PosY: float
    RelPosX: float
    RelPosY: float
    VelX: float
    VelY: float
    HP: int
    Facing: bool
    PlayMakers: NamedStatesContainer

@dataclass
class UserInputs(BaseLayout):
    Horizontal: float
    Vertical: float

    Jump: bool
    Attack: bool
    Heal: bool
    Skill: bool
    Dash: bool
    Harpoon: bool

@dataclass
class FrameData(BaseLayout):
    Hero: Hero
    Enemies: List[Enemy]

@dataclass
class RecordingFrame(BaseLayout):
    FrameData: FrameData
    Targets: UserInputs
    Reward: int

@dataclass
class LiveInferenceFrame(BaseLayout):
    FrameData: FrameData
    Reward: int