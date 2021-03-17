using Lms.Core.Modularity;

namespace Lms.Transaction.Tcc
{
    [DependsOn(typeof(TransactionModule))]
    public class TransactionTccModule : LmsModule
    {
    }
}