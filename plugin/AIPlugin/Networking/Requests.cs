using AIPlugin.Utilities;
using MessagePack;
using System;
using System.Collections.Generic;
namespace AIPlugin.Networking.Requests
{
    [MessagePackObject] public class EmptyPayload : IPayload { }
    [MessagePackObject] public class EmptyResponse : IResponse { }
    public static class Requests
    {
        public class GetArchitectures : RequestHandle<EmptyPayload, GetArchitectures.Response>
        {
            public GetArchitectures(AiGateway gateway) :
                base(gateway, "get_architectures", new EmptyPayload())
            { }
            [MessagePackObject]
            public class Response : IResponse
            {
                [Key("architectures")]
                public Dictionary<string, List<ServerParam>> ArchitectureParams;
            }
        }
        public class GetArchitecturesRefreshable : RefreshableRequest<EmptyPayload, GetArchitectures.Response>
        {
            public GetArchitecturesRefreshable(AiGateway gateway, Func<RequestHandle<EmptyPayload, GetArchitectures.Response>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class GetArtifacts : RequestHandle<EmptyPayload, GetArtifacts.Response>
        {
            public GetArtifacts(AiGateway gateway) :
                base(gateway, "get_artifacts", new EmptyPayload())
            { }
            [MessagePackObject]
            public class Response : IResponse
            {
                [MessagePackObject]
                public class Artifact
                {
                    [Key("architecture_name")] public string ArchitectureName;
                    [Key("artifact_name")] public string Name;
                    [Key("target_boss_name")] public string BossName;
                    [Key("creation_date")] public string CreationDate;
                    [Key("boolean_thresholds")] public List<float> BooleanThresholds;
                    [Key("current_epoch")] public int CurrentEpoch;
                    [Key("loss_history")] public List<(float training, float testing)> LossHistory;
                    [Key("config")] public ArtifactConfig config;
                }
                [MessagePackObject]
                public class ArtifactConfig
                {
                    [Key("architecture_name")] public string Name;
                    //TODO [Key("params")] public Dictionary<string, string> Params;
                }

                [Key("artifacts")] public Dictionary<string, 
                    List<Artifact>> BossArtifacts;
            }
        }
        public class GetArtifactsRefreshable : RefreshableRequest<EmptyPayload, GetArtifacts.Response>
        {
            public GetArtifactsRefreshable(AiGateway gateway, Func<RequestHandle<EmptyPayload, GetArtifacts.Response>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class NewArtifact : RequestHandle<NewArtifact.Payload, EmptyResponse>
        {
            public NewArtifact(AiGateway gateway, Payload payload) :
                base(gateway, "new_artifact", payload)
            { }

            [MessagePackObject]
            public class Payload : IPayload
            {
                [Key("architecture")] public string ArchitectureName { get; set; }
                [Key("target_boss")] public string TargetBossName { get; set; }
                [Key("name")] public string ArtifactName { get; set; }
                [Key("params")] public List<ServerParam> Params { get; set; }
                [Key("overwrite")] public bool Overwrite { get; set; } = false;
                [Key("require_success")] public bool RequireSuccess { get; set; } = true;
                [Key("player_name")] public string PlayerName { get; set; } = "";
                [Key("percent_best")] public float UsePercentBest { get; set; } = 0.8f;

                [Key("delta")] public int Delta = SessionConfig.CaptureFrameDelta;
            }
        }
        public class NewArtifactRefreshable : RefreshableRequest<NewArtifact.Payload, EmptyResponse>
        {
            public NewArtifactRefreshable(AiGateway gateway, Func<RequestHandle<NewArtifact.Payload, EmptyResponse>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class StartBehavioralCloning : RequestHandle<StartBehavioralCloning.Payload, EmptyResponse>
        {
            public StartBehavioralCloning(AiGateway gateway, Payload payload) :
                base(gateway, "bc_start", payload)
            { }
            [MessagePackObject]
            public class Payload : IPayload
            {
                [Key("target_boss_name")] public string TargetBossName { get; set; }
                [Key("artifact_name")] public string ArtifactName { get; set; }
                [Key("num_epochs")] public int NumEpochs { get; set; } = 1;
                [Key("learning_rate")] public float LearningRate { get; set; } = 0.001f;
                [Key("batch_size")] public int BatchSize { get; set; } = 8;
                [Key("use_gpu")] public bool UseGPU { get; set; } = false;
            }
        }
        public class StartBehavioralCloningRefreshable : RefreshableRequest<StartBehavioralCloning.Payload, EmptyResponse>
        {
            public StartBehavioralCloningRefreshable(AiGateway gateway, Func<RequestHandle<StartBehavioralCloning.Payload, EmptyResponse>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class StopBehavioralCloning : RequestHandle<EmptyPayload, EmptyResponse>
        {
            public StopBehavioralCloning(AiGateway gateway) :
                base(gateway, "bc_stop", new EmptyPayload())
            { }
        }
        public class StopBehavioralCloningRefreshable : RefreshableRequest<EmptyPayload, EmptyResponse>
        {
            public StopBehavioralCloningRefreshable(AiGateway gateway, Func<RequestHandle<EmptyPayload, EmptyResponse>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class FinalizeBehavioralCloning : RequestHandle<FinalizeBehavioralCloning.Payload, EmptyResponse>
        {
            public FinalizeBehavioralCloning(AiGateway gateway, Payload payload) :
                base(gateway, "bc_finalize", payload)
            { }
            [MessagePackObject]
            public class Payload : IPayload
            {
                [Key("save")] public bool Save = true;
            }
        }
        public class FinalizeBehavioralCloningRefreshable : RefreshableRequest<FinalizeBehavioralCloning.Payload, EmptyResponse>
        {
            public FinalizeBehavioralCloningRefreshable(AiGateway gateway, Func<RequestHandle<FinalizeBehavioralCloning.Payload, EmptyResponse>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class InitializeInferenceSession : RequestHandle<InitializeInferenceSession.Payload, EmptyResponse>
        {
            public InitializeInferenceSession(AiGateway gateway, Payload payload) :
                base(gateway, "initialize_inference_session", payload)
            { }
            [MessagePackObject]
            public class Payload : IPayload
            {
                [Key("target_boss_name")] public string TargetBossName { get; set; }
                [Key("artifact_name")] public string ArtifactName { get; set; }
            }
        }
        public class InferenceAction: RequestHandle<InferenceFrame, FrameUserInputs>
        {
            public InferenceAction(AiGateway gateway, InferenceFrame payload) :
                base(gateway, "live_inference", payload)
            { }
        }

        public class SetArtifactConfig : RequestHandle<SetArtifactConfig.Payload, EmptyResponse>
        {
            public SetArtifactConfig(AiGateway gateway, Payload payload) :
                base(gateway, "set_artifact_config", payload)
            { }
            [MessagePackObject]
            public class Payload : IPayload
            {
                [Key("target_boss_name")] public string TargetBossName { get; set; }
                [Key("artifact_name")] public string ArtifactName { get; set; }
                [Key("boolean_thresholds")] public List<float> BooleanThresholds { get; set; }
            }
        }
        public class RLStart : RequestHandle<RLStart.Payload, EmptyResponse>
        {
            public RLStart(AiGateway gateway, Payload payload) :
                base(gateway, "rl_start", payload)
            { }
            [MessagePackObject]
            public class Payload : IPayload
            {
                [Key("target_boss_name")] public string TargetBossName { get; set; }
                [Key("artifact_name")] public string ArtifactName { get; set; }
                [Key("num_epochs")] public int NumEpochs { get; set; } = 1;
                [Key("fights_per_epoch")] public int FightsPerEpoch { get; set; } = 1;
            }
        }
        public class RLInferenceAction : RequestHandle<InferenceFrame, FrameUserInputs>
        {
            public RLInferenceAction(AiGateway gateway, InferenceFrame payload) :
                base(gateway, "rl_inference_action", payload)
            { }
        }

        public class RLCanRunFight : RequestHandle<EmptyPayload, RLCanRunFight.Response>
        {
            public RLCanRunFight(AiGateway gateway) :
                base(gateway, "rl_run_fight", new EmptyPayload())
            { }

            [MessagePackObject]
            public class Response : IResponse
            {
                [Key("can_run")] public bool CanRun { get; set; }
                [Key("target_boss_name")] public string TargetBossName { get; set; }
            }
        }
        public class RLFightStartedRefreshable : RefreshableRequest<EmptyPayload, EmptyResponse>
        {
            public RLFightStartedRefreshable(AiGateway gateway, Func<RequestHandle<EmptyPayload, EmptyResponse>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class RLFightFisihed : RequestHandle<EmptyPayload, EmptyResponse>
        {
            public RLFightFisihed(AiGateway gateway) :
                base(gateway, "rl_fight_finished", new EmptyPayload())
            { }
        }
        public class RLFightFisihedRefreshable : RefreshableRequest<EmptyPayload, EmptyResponse>
        {
            public RLFightFisihedRefreshable(AiGateway gateway, Func<RequestHandle<EmptyPayload, EmptyResponse>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }

    }
}
