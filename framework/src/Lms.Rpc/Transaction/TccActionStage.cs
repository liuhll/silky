namespace Lms.Rpc.Transaction
{
    public enum TccActionStage
    {
        PreTry = 0,
        
        Trying,
        
        Confirming,
        
        Canceling,
    }
}