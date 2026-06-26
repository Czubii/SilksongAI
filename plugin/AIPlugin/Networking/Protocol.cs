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
        public class ResponseEnvelope
        {
            [Key("kind")] public string Kind { get; set; } = "response";
            [Key("request_ID")] public string RequestId { get; set; }
            [Key("success")] public bool Success { get; set; }
            [Key("log")] public string ServerLog { get; set; }
            [Key("payload")] public byte[] Payload { get; set; }
        }

        [MessagePackObject]
        public class EventEnvelope
        {
            [Key("kind")] public string Kind { get; set; } = "event";
            [Key("type")] public string EventType { get; set; }
            [Key("payload")] public byte[] Payload { get; set; }
        }
    }

    [MessagePackObject]
    public class ServerParam
    {
        [Key("name")] public string VariableName { get; set; }
        [Key("type")] public string Type { get; set; }
        [Key("value")] public string Value { get; set; }
    }
    public static class EventPayloads
    {
        [MessagePackObject]
        public class BCEpoch
        {
            [Key("current_epoch")] public int CurrentEpoch;
            [Key("start_epoch")] public int StartEpoch;
            [Key("end_epoch")] public int EndEpoch;
            [Key("training_loss")] public float TrainingLoss;
            [Key("testing_loss")] public float TestingLoss;
            [Key("loss_history")] public List<(float training, float testing)> LossHistory;
        }
    }
}
