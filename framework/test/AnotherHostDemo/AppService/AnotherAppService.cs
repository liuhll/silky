using System.Threading.Tasks;
using IAnotherApplication;
using Lms.Transaction.Tcc;

namespace AnotherHostDemo.AppService
{
    public class AnotherAppService : IAnotherAppService
    {
        [TccTransaction(ConfirmMethod = "DeleteConfirm", CancelMethod = "DeleteCancel")]
        public async Task<string> Delete(string name)
        {
            return "Delete " + name;
        }
        
        public async Task<string> DeleteConfirm(string name)
        {
            return "DeleteConfirm " + name;
        }
        
        public async Task<string> DeleteCancel(string name)
        {
            return "DeleteCancel " + name;
        }
    }
}