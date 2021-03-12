using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Lms.Rpc.Transport;

namespace Lms.Rpc.Transaction
{
    public class LmsTransaction : ITransaction
    {
        private IList<IParticipant> m_participants;

        private LmsTransaction()
        {
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
            m_participants = new List<IParticipant>();
        }

        public LmsTransaction(string transId) : this()
        {
            TransId = transId;
        }

        public string TransId { get; }

        private ActionStage _status;
        
        public ActionStage Status
        {
            get => _status;
            set
            {
                var transContext = RpcContext.GetContext().GetTransactionContext();
                if (transContext != null)
                {
                    transContext.Action = value;
                }
                _status = value;
            }
        }

        public TransactionType TransType { get; set; }
        public DateTime CreateTime { get; }
        public DateTime UpdateTime { get; set; }

        public int ReTry { get; set; }

        public IReadOnlyCollection<IParticipant> Participants => m_participants.ToImmutableArray();

        public void RegisterParticipant(IParticipant participant)
        {
            if (participant != null)
            {
                m_participants.Add(participant);
            }
        }

        public void RegisterParticipantList(IEnumerable<IParticipant> participants)
        {
            foreach (var participant in participants)
            {
                RegisterParticipant(participant);
            }
        }
    }
}