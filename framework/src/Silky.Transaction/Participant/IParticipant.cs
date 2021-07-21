using System;
using Silky.Core.DynamicProxy;

namespace Silky.Transaction.Participant
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

        ISilkyMethodInvocation Invocation { get; set; }

        DateTime CreateTime { get; }

        DateTime UpdateTime { get; set; }
    }
}