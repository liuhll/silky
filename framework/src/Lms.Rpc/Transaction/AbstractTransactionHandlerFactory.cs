using System.Collections.Generic;

namespace Lms.Rpc.Transaction
{
    public abstract class AbstractTransactionHandlerFactory : ITransactionHandlerFactory
    {
        protected abstract IDictionary<TransactionRole, ITransactionHandler> Handlers { get; }

        public virtual ITransactionHandler FactoryOf(TransactionContext context)
        {
            if (context == null)
            {
                return Handlers[TransactionRole.Start];
            }

            ITransactionHandler handler = null;
            switch (context.TransactionRole)
            {
                case TransactionRole.Local:
                    handler = Handlers[TransactionRole.Local];
                    break;
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