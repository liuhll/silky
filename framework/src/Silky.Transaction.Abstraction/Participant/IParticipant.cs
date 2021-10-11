using System;
using Newtonsoft.Json;
using Silky.Core.DynamicProxy;

namespace Silky.Transaction.Abstraction.Participant
{
    public interface IParticipant
    {
        string TransId { get; set; }
        string ParticipantId { get; set; }
        string ServiceEntryId { get; set; }
        string ServiceKey { get; set; }
        object[] Parameters { get; set; }
        string HostName { get; set; }
        string ParticipantRefId { get; set; }
        TransactionType TransType { get; set; }
        ActionStage Status { get; set; }

        TransactionRole Role { get; set; }
        int ReTry { get; set; }

        [JsonIgnore] ISilkyMethodInvocation Invocation { get; set; }

        DateTime CreateTime { get; set; }

        DateTime UpdateTime { get; set; }

        int Version { get; set; }
    }
}