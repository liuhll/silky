using System;

namespace Lms.Rpc.Transaction
{
    public class LmsParticipant : IParticipant
    {
        public LmsParticipant()
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
        public int ReTry { get; set; }
        // public string ConfirmMethod { get; set; }
        // public string CancelMethod { get; set; }
        public DateTime CreateTime { get; }
        public DateTime UpdateTime { get; set; }
        
    }
}