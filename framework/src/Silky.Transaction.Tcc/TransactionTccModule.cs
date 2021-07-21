using Silky.Core.Modularity;

namespace Silky.Transaction.Tcc
{
    [DependsOn(typeof(TransactionModule))]
    public class TransactionTccModule : SilkyModule
    {
    }
}