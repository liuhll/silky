using Autofac;
using Silky.Core.Modularity;
using Silky.Transaction.Handler;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Schedule;
using Silky.Transaction.Tcc.Handlers;
using Silky.Transaction.Tcc.Schedule;

namespace Silky.Transaction.Tcc
{
    [DependsOn(typeof(TransactionModule))]
    public class TransactionTccModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<StarterTccTransactionHandler>()
                .InstancePerDependency()
                .Named<ITransactionHandler>(TransactionRole.Start.ToString());

            builder.RegisterType<ParticipantTccTransactionHandler>()
                .InstancePerDependency()
                .Named<ITransactionHandler>(TransactionRole.Participant.ToString());

            builder.RegisterType<TccTransactionRecoveryService>()
                .InstancePerDependency()
                .Named<ITransactionRecoveryService>(TransactionType.Tcc.ToString());
        }
    }
}