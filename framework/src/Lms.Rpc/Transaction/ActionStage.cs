namespace Lms.Rpc.Transaction
{
    public enum ActionStage
    {
        PreTry = 0,
        
        Trying,
        
        Confirming,
        
        Canceling,
    }
}