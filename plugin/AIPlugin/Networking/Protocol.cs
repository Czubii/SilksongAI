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

}
