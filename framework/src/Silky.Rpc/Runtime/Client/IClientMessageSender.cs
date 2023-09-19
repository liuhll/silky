using System;

namespace Silky.Rpc.Runtime.Client;

public interface IClientMessageSender : IMessageSender, IDisposable
{
    bool Enabled { get; }
}