using System;
using Silky.Core.DynamicProxy;

namespace Silky.Transaction.Participant
{
    public class SilkyParticipant : IParticipant
    {
        public SilkyParticipant()
        {
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
        }

        public string TransId { get; set; }
        public string ParticipantId { get; set; }
        public string ParticipantRefId { get; set; }
        public TransactionType TransType { get; set; }
        public ActionStage Status { get; set; }
        
        public TransactionRole Role { get; set; }

        public ParticipantType ParticipantType { get; set; }
        public int ReTry { get; set; }

        public ISilkyMethodInvocation Invocation { get; set; }

        public DateTime CreateTime { get; }
        public DateTime UpdateTime { get; set; }
    }
}