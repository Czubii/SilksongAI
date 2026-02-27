using MessagePack;


namespace AIPlugin
{

    [MessagePackObject]
    public class RecordingInfo
    {
        [Key("Format Version")]
        public int FormatVersion = 2;
        [Key("FrameCount")]
        public int FrameCount;
        [Key("Success")]
        public bool Success;
        [Key("PlayerName")]
        public string PlayerName;
        [Key("BossInternalName")]
        public string BossInternalName;
    }

    [MessagePackObject]
    public class RecordingHeader
    {
        [Key("EnemyNames")]
        public string[] EnemyNames;
    }

    [MessagePackObject]
    public class FrameData //TODO add some header before the frame data to output files
    {
        [Key(0)]
        public FrameHeroData Hero;
        [Key(1)]
        public FrameEnemyData[] Enemies; // The main target (i.e. boss) should always be at the first index
        [Key(2)]
        public FrameUserInputs UserInputs;

    }

    [MessagePackObject]
    public struct FrameEnemyData
    {
        [Key(0)]
        public float posX;
        [Key(1)]
        public float posY;
        [Key(2)]
        public int facing; //(-1)->right (1)->left (based on transform scale.x_scale)
        [Key(3)]
        public float velX;
        [Key(4)]
        public float velY;
        [Key(5)]
        public int hp;
        [Key(6)]
        public PlayMakerData[] playMakers;
    }
    [MessagePackObject]
    public struct PlayMakerData
    {
        [Key(0)]
        public string Name; //PlayMakerFSM.FsmName
        [Key(1)]
        public string StateName; //PlayMakerFSM.ActiveStateName
    }

    [MessagePackObject]
    public struct FrameHeroData // Add position relative to boss
    {
        [Key(0)]
        public float posX;
        [Key(1)]
        public float posY;
        [Key(2)]
        public int facing; //(-1)->right (1)->left (based on transform scale.x_scale)
        [Key(3)]
        public float velX;
        [Key(4)]
        public float velY;
        [Key(5)]
        public int hp;
        [Key(6)]
        public int silk;
        [Key(7)]
        public bool canJump;
        [Key(8)]
        public bool canDoubleJump;
        [Key(9)]
        public bool canAttack;
        [Key(10)]
        public bool canSprint;
        [Key(11)]
        public bool canBind;
        [Key(12)]
        public bool canCast;
        [Key(13)]
        public bool canNailArt;
        [Key(14)]
        public bool canTryHarpoon;
        [Key(15)]
        public bool canInput;
        [Key(16)]
        public bool canBackDash;

    }
    [MessagePackObject]
    public struct FrameUserInputs
    {
        // Movement:
        [Key(0)]
        public bool jump;

        [Key(1)]
        public float left;
        [Key(2)]
        public float right;
        [Key(3)]
        public float up;
        [Key(4)]
        public float down;


        //Actions:
        [Key(5)]
        public bool attack;
        [Key(6)]
        public bool heal;
        [Key(7)]
        public bool skill;
        [Key(8)]
        public bool dash;
        [Key(9)]
        public bool harpoon;

    }


}
