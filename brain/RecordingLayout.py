from dataclasses import dataclass
from typing import List

import numpy as np


###------------------------------------------------------------------------------------------###
#
#   If you plan to add or remove some data in plugin's recorder remember to add them here and always increment
#   SUPPORTED_FORMAT_VERSION (also, the order must match 100%)
#   For string variables (like playmakers) it's not that simple as they must be indexed before we can pass them to neural network.
#   For that to work you will likely need to make some adjustments to Preprocessor class inside Preprocessing.py
#
###------------------------------------------------------------------------------------------###

class LayoutNotSupported(Exception):
    pass

@dataclass
class Layout:

    SUPPORTED_FORMAT_VERSION: int = 11

    @dataclass
    class RecordingFrame:
        num_elements: int = 3
        HERO: int = 0
        ENEMIES: int = 1
        INPUTS: int = 2

    @dataclass
    class Hero:
        POS_X: int = 0
        POS_Y: int = 1
        REL_POS_X: int = 2
        REL_POS_Y: int = 3
        HP: int = 4
        SILK: int = 5
        FACING: int = 6
        IS_STUNNED: int = 7
        CAN_JUMP: int = 8
        CAN_DOUBLE_JUMP: int = 9
        CAN_ATTACK: int = 10
        CAN_SPRINT: int = 11
        CAN_BIND: int = 12
        CAN_CAST: int = 13
        CAN_NAIL_ART: int = 14
        CAN_TRY_HARPOON: int = 15
        CAN_BACK_DASH: int = 16

        num_elements = 16

        # 1 for boolean-type variables 0 for others - this is used in preprocessor in z-score
        # standardization to affect only the non-boolean variables
        boolean_mask = np.array([
            0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
        ], dtype=np.bool)

    @dataclass
    class Enemy:
        POS_X: int = 0
        POS_Y: int = 1
        REL_POS_X: int = 2
        REL_POS_Y: int = 3
        VEL_X: int = 4
        VEL_Y: int = 5
        HP: int = 6
        FACING: int = 7
        NAME: int = 8
        # Very important - this has to ALWAYS be last and should be included in boolean mask as boolean-type !!!!!!!!!!!!!!!!!!!!!!!!!!!!
        PLAYMAKERS: int = 9
        # !!!!!!!!!!!!!!!!!

        num_elements = 10

        # 1 for boolean-type variables 0 for others - this is used in preprocessor in z-score
        # standardization to affect only the non-boolean variables
        boolean_mask = np.array([
            0, 0, 0, 0, 0, 0, 0, 1, 1, 1
        ], dtype=np.bool)

    @dataclass
    class Inputs:
        num_elements: int = 8
        float_values: int = 2

        HORIZONTAL: int = 0
        VERTICAL: int = 1
        JUMP: int = 2   #bool

        ATTACK: int = 3 #bool
        HEAL: int = 4   #bool
        SKILL: int = 5  #bool
        DASH: int = 6   #bool
        HARPOON: int = 7#bool

        # no boolean mask here as this won't be standardised???

    @dataclass
    class Playmaker:
        num_elements: int = 2
        NAME: int = 0
        STATE_NAME: int = 1

        # no boolean mask here as this won't be standard