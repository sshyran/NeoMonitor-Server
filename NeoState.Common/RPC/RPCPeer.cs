﻿using Newtonsoft.Json;

namespace NeoState.Common.RPC
{
    public class RPCPeer
    {
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        [JsonProperty(PropertyName = "port")]
        public uint Port { get; set; }
    }
}
