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
                [Key("artifacts")] public Dictionary<string, List<string>> BossArtifacts;
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
                base(gateway, "start_training", payload)
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
                base(gateway, "stop_training", new EmptyPayload())
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
                base(gateway, "finalize_training", payload)
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
        public class GetInferenceArtifact : RequestHandle<EmptyPayload, GetInferenceArtifact.Response>
        {
            public GetInferenceArtifact(AiGateway gateway) :
                base(gateway, "get_inference_artifact", new EmptyPayload())
            { }
            [MessagePackObject]
            public class Response : IResponse
            {
                [Key("target_boss_name")] public string TargetBossName { get; set; }
                [Key("artifact_name")] public string ArtifactName { get; set; }
            }
        }
        public class GetInferenceArtifactRefreshable : RefreshableRequest<EmptyPayload, GetInferenceArtifact.Response>
        {
            public GetInferenceArtifactRefreshable(AiGateway gateway, 
                Func<RequestHandle<EmptyPayload, GetInferenceArtifact.Response>> requestFactory) :
                base(gateway, requestFactory)
            { }
        }
        public class SetInferenceArtifact : RequestHandle<SetInferenceArtifact.Payload, EmptyResponse>
        {
            public SetInferenceArtifact(AiGateway gateway, Payload payload) :
                base(gateway, "set_inference_artifact", payload)
            { }
            [MessagePackObject]
            public class Payload : IPayload
            {
                [Key("target_boss_name")] public string TargetBossName { get; set; }
                [Key("artifact_name")] public string ArtifactName { get; set; }
            }
        }
        public class LiveInference: RequestHandle<InferenceFrame, FrameUserInputs>
        {
            public LiveInference(AiGateway gateway, InferenceFrame payload) :
                base(gateway, "live_inference", payload)
            { }
        }

    }
}
