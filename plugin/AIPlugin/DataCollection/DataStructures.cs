using AIPlugin.BossfightSession;
using AIPlugin.Utilities;
using MessagePack;


namespace AIPlugin //TODO fix all namespaces
{
    [MessagePackObject]
    public class RecordingHeader
    {
        [Key("format_version")] public int FormatVersion = 11;
        [Key("record_frame_delta")] public int RecordFrameDelta = SessionConfig.CaptureFrameDelta;
        [Key("target_name")] public string BossName;
        [Key("enemy_names")] public string[] EnemyNames;
        [Key("player_name")] public string PlayerName;
    }
    [MessagePackObject]
    public class RecordingFooter
    {
        [Key("success")] public bool Success;
        [Key("frame_count")] public int FrameCount;
        [Key("total_reward")] public int TotalReward;
    }

    [MessagePackObject]
    public class RecordingFrameData
    {
        [Key(0)] public FrameHeroData Hero;
        [Key(1)] public FrameEnemyData[] Enemies; // The main target (i.e. boss) should always be at the first index
        [Key(2)] public FrameUserInputs UserInputs;
        [Key(3)] public int Reward = 0;

    }
    [MessagePackObject]
    public class LivePredictionFrameData
    {
        [Key(0)] public FrameHeroData Hero;
        [Key(1)] public FrameEnemyData[] Enemies; // The main target (i.e. boss) should always be at the first index
        [Key(2)] public int Reward = 0;
        public static LivePredictionFrameData FromRecordingFrameData(RecordingFrameData data)
        {
            return new LivePredictionFrameData()
            {
                Hero = data.Hero,
                Enemies = data.Enemies,
                Reward = data.Reward,
            };
        }
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
        [Key(4)] public int hp;
        [Key(5)] public int silk;
        [Key(6)] public bool facing; //(true)->x_scale >= 0
        [Key(7)] public bool IsStunned;
        [Key(8)] public bool canJump;
        [Key(9)] public bool canDoubleJump;
        [Key(10)] public bool canAttack;
        [Key(11)] public bool canSprint;
        [Key(12)] public bool canBind;
        [Key(13)] public bool canCast;
        [Key(14)] public bool canNailArt;
        [Key(15)] public bool canTryHarpoon;
        [Key(16)] public bool canBackDash;
    }
    [MessagePackObject]
    public class FrameUserInputs
    {
        // Movement:
        [Key(0)] public float horizontal;
        [Key(1)] public float vertical;
        [Key(2)] public bool jump;
        //Actions:
        [Key(3)] public bool attack;
        [Key(4)] public bool heal;
        [Key(5)] public bool skill;
        [Key(6)] public bool dash;
        [Key(7)] public bool harpoon;
    }
}