using System;

namespace Lms.Rpc.Transaction
{
    public interface IParticipant
    {
        string TransId { get; set; }
        string ParticipantId { get; set; }
        string ParticipantRefId { get; set; }
        TransactionType TransType { get; set; }
        ActionStage Status { get; set; }

        TransactionRole Role { get; set; }

        int ReTry { get; set; }

        string ConfirmMethod { get; set; }

        string CancelMethod { get; set; }
        
        DateTime CreateTime { get; }

        DateTime UpdateTime { get; set; }
    }
}