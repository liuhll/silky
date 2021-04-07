using Silky.Lms.Core.Modularity;

namespace Silky.Lms.Transaction.Tcc
{
    [DependsOn(typeof(TransactionModule))]
    public class TransactionTccModule : LmsModule
    {
    }
}