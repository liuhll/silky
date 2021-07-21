using System.Collections.Generic;
using Silky.Rpc.Runtime.Server;

namespace Silky.Transaction.Handler
{
    public abstract class AbstractTransactionHandlerFactory : ITransactionHandlerFactory
    {
        protected abstract IDictionary<TransactionRole, ITransactionHandler> Handlers { get; }
        
        public abstract ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);

    }
}