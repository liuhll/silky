namespace Silky.Transaction
{
    public enum ActionStage
    {
        PreTry = 0, // 准备开始执行方法

        Trying, // Try方法执行完成

        Confirming,
        
        Confirmed, //Confirm执行完成

        Canceling, //准备开始执行Cancel

        Canceled, // Canceled执行完成
        
        Error, //出现异常
    }
}