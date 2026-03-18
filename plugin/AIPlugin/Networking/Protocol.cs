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
            [Key("request_ID")] public string RequestId { get; set; }
            [Key("type")] public string Type { get; set; }
            [Key("success")] public bool Success { get; set; }
            [Key("log")] public string ServerLog { get; set; }
            [Key("payload")] public T Payload { get; set; }
        }
    }

    [MessagePackObject]
    public class ServerFunctionParam
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
        new_model
    }

    public static class Requests
    {
        [MessagePackObject]
        public class NewModel
        {
            [Key("architecture")] public string ArchitectureName { get; set; }
            [Key("target_boss")] public string TargetBossName { get; set; }
            [Key("name")] public string ModelName { get; set; }
            [Key("params")] public List<ServerFunctionParam> Params { get; set; }
            [Key("overwrite")] public bool Overwrite { get; set; } = false;
            [Key("require_success")] public bool RequireSuccess { get; set; } = true;
            [Key("player_name")] public string PlayerName { get; set; } = "";
            [Key("percent_best")] public float UsePercentBest { get; set; } = 0.8f;

            [Key("delta")] public int Delta = SessionConfig.CaptureFrameDelta;

        }
    }
    public static class Responses
    {
        [MessagePackObject]
        public class Architectures
        {
            [Key("architectures")]
            public Dictionary<string, List<ServerFunctionParam>> ArchitectureParams;
        }
    }
}
