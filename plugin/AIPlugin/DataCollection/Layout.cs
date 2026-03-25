using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using AIPlugin.Utilities;
using MessagePack;


namespace AIPlugin //TODO fix all namespaces
{
    [MessagePackObject]
    public class RecordingHeader
    {
        [Key("format_version")] public int FormatVersion = 17;
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
    public class FrameData
    {
        [Key(0)] public FrameHeroData Hero;
        [Key(1)] public FrameEnemyData[] Enemies; // The main target (i.e. boss) should always be at the first index
    }

    [MessagePackObject]
    public class RecordingFrame
    {
        [Key(0)] public FrameData Data;
        [Key(1)] public FrameUserInputs UserInputs;
        [Key(2)] public int Reward = 0;

    }
    [MessagePackObject]
    public class InferenceFrame: IPayload
    {
        [Key(0)] public FrameData Data;
        [Key(1)] public int Reward = 0;
        public static InferenceFrame FromRecordingFrameData(RecordingFrame recordingFrame)
        {
            return new InferenceFrame()
            {
                Data = recordingFrame.Data,
                Reward = recordingFrame.Reward,
            };
        }
    }

    [MessagePackObject]
    public struct FrameEnemyData
    {
        [Key(0)] public float PosX;
        [Key(1)] public float PosY;
        [Key(2)] public float RelPosX; // relative to hero
        [Key(3)] public float RelPosY; 
        [Key(4)] public float VelX;
        [Key(5)] public float VelY;
        [Key(6)] public int HP;
        [Key(7)] public bool Facing; //(true)->x_scale >= 0
        [Key(8)] public NamedStatesContainer PlayMakers; //PlayMakerFSM.FsmName and PlayMakerFSM.ActiveStateName
    }
    [MessagePackObject]
    public struct NamedStatesContainer
    {
        [Key(0)] public string ParentName;
        [Key(1)] public NamedState[] NamedStates;
    }

    [MessagePackObject]
    public struct NamedState
    {
        [Key(0)] public string Name; 
        [Key(1)] public string StateName; 
    }

    [MessagePackObject]
    public struct FrameHeroData 
    {
        [Key(0)] public float PosX;
        [Key(1)] public float PosY;
        [Key(2)] public float RelPosX; // relative to targetEnemy
        [Key(3)] public float RelPosY;
        [Key(4)] public int HP;
        [Key(5)] public int Silk;
        [Key(6)] public bool IsStunned;
        [Key(7)] public bool CanJump;
        [Key(8)] public bool CanDoubleJump;
        [Key(9)] public bool CanAttack;
        [Key(10)] public bool CanSprint;
        [Key(11)] public bool CanBind;
        [Key(12)] public bool CanCast;
        [Key(13)] public bool CanNailArt;
        [Key(14)] public bool CanTryHarpoon;
        [Key(15)] public bool CanBackDash;
        [Key(16)] public float InvDistX;
        [Key(17)] public float InvDistY;
    }
    [MessagePackObject]
    public class FrameUserInputs: IResponse
    {
        // Movement:
        [Key(0)] public float Horizontal;
        [Key(1)] public float Vertical;
        [Key(2)] public bool Jump;
        //Actions:
        [Key(3)] public bool Attack;
        [Key(4)] public bool Heal;
        [Key(5)] public bool Skill;
        [Key(6)] public bool Dash;
        [Key(7)] public bool Harpoon;
    }
}