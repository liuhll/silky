using System;
using Silky.Core.DynamicProxy;

namespace Silky.Transaction.Repository.Spi.Participant
{
    [Serializable]
    public class SilkyParticipant : IParticipant
    {
        public SilkyParticipant()
        {
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
        }

        [NonSerialized] private ISilkyMethodInvocation _invocation;

        public string TransId { get; set; }
        public string ParticipantId { get; set; }
        public string ParticipantRefId { get; set; }
        public TransactionType TransType { get; set; }
        public ActionStage Status { get; set; }

        public TransactionRole Role { get; set; }

        public ParticipantType ParticipantType { get; set; }

        public int ReTry { get; set; }
        
        public ISilkyMethodInvocation Invocation
        {
            get => _invocation;
            set => _invocation = value;
        }

        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}