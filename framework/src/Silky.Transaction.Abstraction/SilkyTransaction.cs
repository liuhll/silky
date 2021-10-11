using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Silky.Core;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Abstraction
{
    public class SilkyTransaction : ITransaction
    {
        private IList<IParticipant> m_participants;


        public SilkyTransaction()
        {
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
            Version = 1;
            HostName = EngineContext.Current.HostName;
            m_participants = new List<IParticipant>();
        }

        public SilkyTransaction(string transId) : this()
        {
            TransId = transId;
        }

        public string TransId { get; set; }

        public string HostName { get; set; }

        public ActionStage Status { get; set; }

        public TransactionType TransType { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        public int ReTry { get; set; }

        public int Version { get; set; }

        [JsonIgnore] public IReadOnlyCollection<IParticipant> Participants => m_participants.ToImmutableArray();

        public void RegisterParticipant(IParticipant participant)
        {
            if (participant != null)
            {
                m_participants.Add(participant);
                //await TransRepS
            }
        }

        public void RemoveParticipant(IParticipant participant)
        {
            if (participant != null)
            {
                m_participants.Remove(participant);
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