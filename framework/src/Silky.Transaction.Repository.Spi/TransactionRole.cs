namespace Silky.Transaction.Repository.Spi
{
    public enum TransactionRole
    {
        Start = 0,

        Participant,
        
        Local,

        Consumer,
    }
}