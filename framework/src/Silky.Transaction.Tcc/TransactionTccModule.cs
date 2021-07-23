using Autofac;
using Silky.Core.Modularity;
using Silky.Transaction.Handler;
using Silky.Transaction.Tcc.Handlers;

namespace Silky.Transaction.Tcc
{
    [DependsOn(typeof(TransactionModule))]
    public class TransactionTccModule : SilkyModule
    {
        protected override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<StarterTccTransactionHandler>()
                .InstancePerLifetimeScope()
                .Named<ITransactionHandler>(TransactionRole.Start.ToString());
            
            builder.RegisterType<ConsumerTccTransactionHandler>()
                .InstancePerLifetimeScope()
                .Named<ITransactionHandler>(TransactionRole.Consumer.ToString());
            
            builder.RegisterType<ParticipantTccTransactionHandler>()
                .InstancePerLifetimeScope()
                .Named<ITransactionHandler>(TransactionRole.Participant.ToString());
            builder.RegisterType<LocalTccTransactionHandler>()
                .InstancePerLifetimeScope()
                .Named<ITransactionHandler>(TransactionRole.Local.ToString());
        }
    }
}