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
            [Key("error_message")] public string ErrorMessage { get; set; }
            [Key("payload")] public T Payload { get; set; }
        }
    }
    [MessagePackObject]
    public class Empty { }


    public static class Payloads
    {
        [MessagePackObject]
        public class ModelsOverviewResponse
        {
            [Key("selected_model")]
            public List<string> SelectedModel { get; set; }
            [Key("models")]
            public Dictionary<string, List<string>> Models { get; set; }
        }
        [MessagePackObject]
        public class SelectModelRequest
        {
            [Key("boss_name")]
            public string BossName;
            [Key("model_name")]
            public string ModelName;
        }

        [MessagePackObject]
        public class FunctionParam
        {
            [Key("name")] public string VariableName { get; set; }
            [Key("type")] public string Type { get; set; }
            [Key("value")] public string Value { get; set; }
        }

        [MessagePackObject]
        public class GetArchitecturesResponse
        {
            [Key("architectures")]
            public Dictionary<string, List<FunctionParam>> ArchitectureParams;
        }

        [MessagePackObject]
        public class NewModelRequest
        {
            [Key("architecture")] public string ArchitectureName { get; set; }
            [Key("target_boss")] public string TargetBossName { get; set; }
            [Key("name")] public string ModelName { get; set; }
            [Key("params")] public List<FunctionParam> Params { get; set; }
            [Key("override")] public bool Overwrite {  get; set; } = false;
            [Key("require_success")] public bool RequireSuccess { get; set; } = true;
            [Key("player_name")] public string PlayerName { get; set; } = "";
            [Key("percent_best")] public float UsePercentBest { get; set; } = 0.8f;

        }
    }
}
