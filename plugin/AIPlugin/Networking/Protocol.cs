using AIPlugin.Utilities;
using MessagePack;
using System.Collections.Generic;
namespace AIPlugin.Networking
{
    public class Protocol
    {
        [MessagePackObject]
        public class RequestEnvelope<T>
        {
            [Key("request_ID")] public string RequestId { get; set; }
            [Key("type")] public string Type { get; set; }
            [Key("payload")] public T Payload { get; set; }
        }
        [MessagePackObject]
        public class ResponseEnvelope<T>
        {
            [Key("kind")] public string Kind { get; set; } = "response";
            [Key("request_ID")] public string RequestId { get; set; }
            [Key("success")] public bool Success { get; set; }
            [Key("log")] public string ServerLog { get; set; }
            [Key("payload")] public T Payload { get; set; }
        }

        [MessagePackObject]
        public class EventEnvelope
        {
            [Key("kind")] public string Kind { get; set; } = "event";
            [Key("type")] public string EventType { get; set; }
            //[Key("payload")] public T Payload { get; set; } ADD IF NEEDED
        }
    }

    [MessagePackObject]
    public class ArchitectureConstructorParams
    {
        [Key("name")] public string VariableName { get; set; }
        [Key("type")] public string Type { get; set; }
        [Key("value")] public string Value { get; set; }
    }


    [MessagePackObject]
    public class EmptyPayload { }

    public enum RequestType
    {
        get_architectures,
        get_artifacts,
        new_model,
        train_artifact
    }

    public static class Requests
    {
        [MessagePackObject]
        public class NewArtifact
        {
            [Key("architecture")] public string ArchitectureName { get; set; }
            [Key("target_boss")] public string TargetBossName { get; set; }
            [Key("name")] public string ArtifactName { get; set; }
            [Key("params")] public List<ArchitectureConstructorParams> Params { get; set; }
            [Key("overwrite")] public bool Overwrite { get; set; } = false;
            [Key("require_success")] public bool RequireSuccess { get; set; } = true;
            [Key("player_name")] public string PlayerName { get; set; } = "";
            [Key("percent_best")] public float UsePercentBest { get; set; } = 0.8f;

            [Key("delta")] public int Delta = SessionConfig.CaptureFrameDelta;

        }

        [MessagePackObject]
        public class TrainArtifcat
        {
            [Key("target_boss_name")] public string TargetBossName { get; set; }
            [Key("artifact_name")] public string ArtifactName { get; set; }
            [Key("num_epochs")] public int NumEpochs { get; set; } = 1;
            [Key("learning_rate")] public float LearningRate { get; set; } = 0.001f;
            [Key("batch_size")] public int BatchSize { get; set; } = 8;
            [Key("use_gpu")] public bool UseGPU { get; set; } = false;
        }
    }
    public static class Responses
    {
        [MessagePackObject]
        public class Architectures
        {
            [Key("architectures")] 
            public Dictionary<string, List<ArchitectureConstructorParams>> ArchitectureParams;
        }

        [MessagePackObject]
        public class Artifacts
        {
            [Key("artifacts")] public Dictionary<string, List<string>> BossArtifacts;
        }
    }
}
