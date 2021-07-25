using System;
using Newtonsoft.Json;
using Silky.Core;
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
            AppName = EngineContext.Current.AppName;
        }

        public string TransId { get; set; }
        public string ParticipantId { get; set; }

        public string ServiceId { get; set; }

        public string ServiceKey { get; set; }
        public object[] Parameters { get; set; }
        

        public string AppName { get; set; }
        public string ParticipantRefId { get; set; }
        public TransactionType TransType { get; set; }
        public ActionStage Status { get; set; }

        public TransactionRole Role { get; set; }

        public int ReTry { get; set; }

        [JsonIgnore] public ISilkyMethodInvocation Invocation { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}