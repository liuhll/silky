namespace Lms.Rpc.Transaction
{
    public enum TransactionRole
    {
        Start = 0,

        Participant,

        Consumer,

        Local,

        Inline
    }
}