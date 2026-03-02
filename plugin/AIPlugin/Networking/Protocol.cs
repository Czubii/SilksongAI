using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Networking
{
    public class Protocol
    {
        [MessagePackObject]
        public class RequestEnvelope
        {
            [Key("RequestId")] public string RequestId { get; set; }
            [Key("Type")] public string Type { get; set; }
            [Key("Payload")] public object Payload { get; set; }
        }
        [MessagePackObject]
        public class ResponseEnvelope
        {
            [Key("RequestId")] public string RequestId { get; set; }
            [Key("Type")] public string Type { get; set; }
            [Key("Success")] public bool Success { get; set; }
            [Key("Payload")] public object Payload { get; set; }
        }
    }
}
