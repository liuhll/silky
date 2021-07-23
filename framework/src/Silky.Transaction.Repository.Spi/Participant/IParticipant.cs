using System;
using Newtonsoft.Json;
using Silky.Core.DynamicProxy;

namespace Silky.Transaction.Repository.Spi.Participant
{
    public interface IParticipant
    {
        string TransId { get; set; }
        string ParticipantId { get; set; }
        string ParticipantRefId { get; set; }
        TransactionType TransType { get; set; }
        ActionStage Status { get; set; }

        TransactionRole Role { get; set; }

        ParticipantType ParticipantType { get; set; }

        int ReTry { get; set; }

        [JsonIgnore]
        ISilkyMethodInvocation Invocation { get; set; }

        DateTime CreateTime { get; }

        DateTime UpdateTime { get; set; }
    }
}