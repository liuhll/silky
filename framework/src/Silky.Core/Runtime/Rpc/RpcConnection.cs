namespace Silky.Core.Runtime.Rpc
{
    public class RpcConnection
    {
        public string ClientHost { get; internal set; }

        public int? RemotePort { get; internal set; }

        internal int ClientPort { get; set; }

        internal ServiceProtocol ClientServiceProtocol { get; set; }

        internal string LocalHost { get; set; }

        internal int LocalPort { get; set; }

        internal ServiceProtocol LocalServiceProtocol { get; set; }

        public string ClientAddress => $"{ClientHost}:{ClientPort}";

        public string LocalAddress => $"{LocalHost}:{LocalPort}";

        public string ClientRpcEndpoint => $"{ClientServiceProtocol}://{ClientHost}:{ClientPort}";

        public string LocalRpcEndpoint => $"{LocalServiceProtocol}://{LocalHost}:{LocalPort}";
    }
}