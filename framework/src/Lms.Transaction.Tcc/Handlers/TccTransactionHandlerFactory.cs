using System.Collections.Generic;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transaction;

namespace Lms.Transaction.Tcc.Handlers
{
    public class TccTransactionHandlerFactory : AbstractTransactionHandlerFactory, ITransientDependency
    {
        private static IDictionary<TransactionRole, ITransactionHandler> _handlersMap =
            new Dictionary<TransactionRole, ITransactionHandler>();

        static TccTransactionHandlerFactory()
        {
            _handlersMap.Add(TransactionRole.Start, new StarterTccTransactionHandler());
            _handlersMap.Add(TransactionRole.Participant, new ParticipantTccTransactionHandler());
            _handlersMap.Add(TransactionRole.Consumer, new ConsumerTccTransactionHandler());
        }

        protected override IDictionary<TransactionRole, ITransactionHandler> Handlers => _handlersMap;

        public override ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry,
            string serviceKey)
        {
            if (context == null)
            {
                var tccTransactionProvider = serviceEntry.GetTccTransactionProvider(serviceKey);
                if (tccTransactionProvider != null)
                {
                    return Handlers[TransactionRole.Start];
                }
                return null;
            }

            ITransactionHandler handler = null;
            switch (context.TransactionRole)
            {
                case TransactionRole.Participant:
                case TransactionRole.Start:
                    handler = Handlers[TransactionRole.Participant];
                    break;
                default:
                    handler = Handlers[TransactionRole.Consumer];
                    break;
            }

            return handler;
        }
    }
}