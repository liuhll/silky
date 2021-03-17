namespace Lms.Transaction
{
    public enum ActionStage
    {
        PreTry = 0,
        
        Trying,
        
        Confirming,
        
        Canceling,
    }
}