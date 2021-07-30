namespace Silky.Rpc.Runtime.Server
{
    public interface ITccTransactionProvider
    { 
        string ConfirmMethod { get; set; }
        
        string CancelMethod { get; set; }
    }
}