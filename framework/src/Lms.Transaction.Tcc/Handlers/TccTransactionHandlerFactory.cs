using System.Collections.Generic;
using Lms.Core.DependencyInjection;
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
            _handlersMap.Add(TransactionRole.Local, new LocalTccTransactionHandler());
            _handlersMap.Add(TransactionRole.Participant, new ParticipantTccTransactionHandler());
            _handlersMap.Add(TransactionRole.Consumer, new ConsumerTccTransactionHandler());
        }

        protected override IDictionary<TransactionRole, ITransactionHandler> Handlers => _handlersMap;
    }
}