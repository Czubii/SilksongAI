from dataclasses import dataclass
from typing import List

###------------------------------------------------------------------------------------------###
#
#   If you plan to add or remove some data in plugin's recorder remember to add them here. Also, the order must match 100%
#   For string variables (like playmakers) it's not that simple as they must be indexed before we can pass them to neural network.
#   For that to work you will likely need to make some adjustments to Preprocessor class inside Preprocessing.py
#
###------------------------------------------------------------------------------------------###

# The current version of the data structure this script supports

@dataclass
class Layout:

    SUPPORTED_FORMAT_VERSION: int = 2

    @dataclass
    class Frame:
        HERO: int = 0
        ENEMIES: int = 1
        INPUTS: int = 2

    @dataclass
    class Hero:
        num_elements = 17
        POS_X: int = 0
        POS_Y: int = 1
        FACING: int = 2
        VEL_X: int = 3
        VEL_Y: int = 4
        HP: int = 5
        SILK: int = 6
        CAN_JUMP: int = 7
        CAN_DOUBLE_JUMP: int = 8
        CAN_ATTACK: int = 9
        CAN_SPRINT: int = 10
        CAN_BIND: int = 11
        CAN_CAST: int = 12
        CAN_NAIL_ART: int = 13
        CAN_TRY_HARPOON: int = 14
        CAN_INPUT: int = 15
        CAN_BACK_DASH: int = 16

    @dataclass
    class Enemy:
        num_elements = 7
        POS_X: int = 0
        POS_Y: int = 1
        FACING: int = 2
        VEL_X: int = 3
        VEL_Y: int = 4
        HP: int = 5
        PLAY_MAKERS: int = 6 # Very important - this has to ALWAYS be last !!!!!!!!!!!!!!!!!!!!!!!!!!!!


    @dataclass
    class Inputs:
        JUMP: int = 0

        LEFT: int = 1
        RIGHT: int = 2
        UP: int = 3
        DOWN: int = 4

        ATTACK: int = 5
        HEAL: int = 6
        SKILL: int = 7
        DASH: int = 8
        HARPOON: int = 9

    @dataclass
    class Playmaker:
        NAME: int = 0
        STATE_NAME: int = 1