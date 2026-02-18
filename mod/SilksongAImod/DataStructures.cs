using MessagePack;


namespace SilksongAI
{
    [MessagePackObject]
    public struct RecordingInfo
    {
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
    public struct FrameData
    {
        [Key(0)]
        public EnemyData enemy;
        [Key(1)]
        public HeroData hero;
        [Key(2)]
        public UserInputs userInputs;

    }

    [MessagePackObject]
    public struct EnemyData
    {
        [Key(0)]
        public float posX;
        [Key(1)]
        public float posY;
        [Key(2)]
        public float velX;
        [Key(3)]
        public float velY;
        [Key(4)]
        public int hp;

    }
    [MessagePackObject]
    public struct HeroData
    {
        [Key(0)]
        public float posX;
        [Key(1)]
        public float posY;
        [Key(2)]
        public float velX;
        [Key(3)]
        public float velY;
        [Key(4)]
        public int hp;
        [Key(5)]
        public int silk;
        [Key(6)]
        public bool canJump;
    }
    [MessagePackObject]
    public struct UserInputs
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
