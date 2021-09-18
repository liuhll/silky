using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.DynamicProxy;
using Silky.Core.Logging;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Handler;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Diagnostics;
using Silky.Transaction.Tcc.Executor;

namespace Silky.Transaction.Tcc.Handlers
{
    public class StarterTccTransactionHandler : ITransactionHandler
    {
        private readonly TccTransactionExecutor executor = TccTransactionExecutor.Executor;

        private static readonly DiagnosticListener s_diagnosticListener =
            new(TransactionDiagnosticListenerNames.DiagnosticGlobalTransactionListener);

        public ILogger<StarterTccTransactionHandler> Logger { get; set; }

        public StarterTccTransactionHandler()
        {
            Logger = NullLogger<StarterTccTransactionHandler>.Instance;
        }

        public async Task Handler(TransactionContext context, ISilkyMethodInvocation invocation)
        {
            try
            {
                var serviceEntry = invocation.GetServiceEntry();
                var transaction = await executor.PreTry(invocation);
                WriteTracing(TransactionDiagnosticListenerNames.GlobalPreTryHandle, transaction, serviceEntry);
                SilkyTransactionHolder.Instance.Set(transaction);
                var transactionContext = new TransactionContext
                {
                    Action = ActionStage.Trying,
                    TransId = transaction.TransId,
                    TransactionRole = TransactionRole.Start,
                    TransType = TransactionType.Tcc
                };
                SilkyTransactionContextHolder.Instance.Set(transactionContext);
                try
                {
                    await invocation.ProceedAsync();
                    transaction.Status = ActionStage.Trying;
                    await executor.UpdateStartStatus(transaction);
                    WriteTracing(TransactionDiagnosticListenerNames.GlobalTryingHandle, transaction, serviceEntry);
                }
                catch (Exception ex)
                {
                    var errorCurrentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                    WriteTracing(TransactionDiagnosticListenerNames.GlobalCancelingHandle, errorCurrentTransaction,
                        serviceEntry);
                    await executor.GlobalCancel(errorCurrentTransaction);
                    WriteTracing(TransactionDiagnosticListenerNames.GlobalCancelingHandle, errorCurrentTransaction,
                        serviceEntry);
                    Logger.LogException(ex);
                    throw;
                }

                var currentTransaction = SilkyTransactionHolder.Instance.CurrentTransaction;
                WriteTracing(TransactionDiagnosticListenerNames.GlobalConfirmingHandle, currentTransaction,
                    serviceEntry);
                await executor.GlobalConfirm(currentTransaction);
                WriteTracing(TransactionDiagnosticListenerNames.GlobalConfirmedHandle, currentTransaction,
                    serviceEntry);
            }
            finally
            {
                SilkyTransactionContextHolder.Instance.Remove();
                executor.Remove();
            }
        }

        private void WriteTracing(string tracingName, ITransaction transaction, ServiceEntry serviceEntry)
        {
            if (s_diagnosticListener.IsEnabled(tracingName))
            {
                s_diagnosticListener.Write(tracingName, new GlobalTransactionEventData()
                {
                    ServiceEntryId = serviceEntry.Id,
                    Transaction = transaction,
                    Role = TransactionRole.Start,
                    Type = TransactionType.Tcc
                });
            }
        }
    }
}