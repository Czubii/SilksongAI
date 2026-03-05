using AIPlugin.BossfightSession;
using MessagePack;


namespace AIPlugin
{
    [MessagePackObject]
    public class RecordingHeader
    {
        [Key("FormatVersion")] public int FormatVersion = 9;
        [Key("RecordFrameDelta")] public int RecordFrameDelta = SessionConfig.RecordFrameDelta;
        [Key("BossName")] public string BossName;
        [Key("EnemyNames")] public string[] EnemyNames;
        [Key("PlayerName")] public string PlayerName;
    }
    [MessagePackObject]
    public class RecordingFooter
    {
        [Key("Success")] public bool Success;
        [Key("FrameCount")] public int FrameCount;
    }

    [MessagePackObject]
    public class RecordingFrameData
    {
        [Key(0)] public FrameHeroData Hero;
        [Key(1)] public FrameEnemyData[] Enemies; // The main target (i.e. boss) should always be at the first index
        [Key(2)] public FrameUserInputs UserInputs;

    }
    [MessagePackObject]
    public class LivePredictionFrameData
    {
        [Key(0)] public FrameHeroData Hero;
        [Key(1)] public FrameEnemyData[] Enemies; // The main target (i.e. boss) should always be at the first index
    }

    [MessagePackObject]
    public struct FrameEnemyData
    {
        [Key(0)] public float posX;
        [Key(1)] public float posY;
        [Key(2)] public float RelPosX; // relative to hero
        [Key(3)] public float RelPosY; 
        [Key(4)] public float velX;
        [Key(5)] public float velY;
        [Key(6)] public int hp;
        [Key(7)] public bool facing; //(true)->x_scale >= 0
        [Key(8)] public string Name;
        [Key(9)] public PlayMakerData[] playMakers;
    }
    [MessagePackObject]
    public struct PlayMakerData
    {
        [Key(0)] public string Name; //PlayMakerFSM.FsmName
        [Key(1)] public string StateName; //PlayMakerFSM.ActiveStateName
    }

    [MessagePackObject]
    public struct FrameHeroData 
    {
        [Key(0)] public float posX;
        [Key(1)] public float posY;
        [Key(2)] public float RelPosX; // relative to targetEnemy
        [Key(3)] public float RelPosY;
        [Key(4)] public float velX;
        [Key(5)] public float velY;//TODO FIX KEYS
        [Key(6)] public int hp;
        [Key(7)] public int silk;
        [Key(8)] public bool facing; //(true)->x_scale >= 0
        [Key(9)] public bool IsStunned;
        [Key(10)] public bool canJump;
        [Key(11)] public bool canDoubleJump;
        [Key(12)] public bool canAttack;
        [Key(13)] public bool canSprint;
        [Key(14)] public bool canBind;
        [Key(15)] public bool canCast;
        [Key(16)] public bool canNailArt;
        [Key(17)] public bool canTryHarpoon;
        [Key(18)] public bool canInput;
        [Key(19)] public bool canBackDash;
    }
    [MessagePackObject]
    public class FrameUserInputs
    {
        // Movement:
        [Key(0)] public float left;
        [Key(1)] public float right;
        [Key(2)] public float up;
        [Key(3)] public float down;
        [Key(4)] public bool jump;
        //Actions:
        [Key(5)] public bool attack;
        [Key(6)] public bool heal;
        [Key(7)] public bool skill;
        [Key(8)] public bool dash;
        [Key(9)] public bool harpoon;
    }
}