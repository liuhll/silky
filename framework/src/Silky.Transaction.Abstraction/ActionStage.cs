namespace Silky.Transaction.Abstraction
{
    public enum ActionStage
    {
        PreTry = 0, // Pre try tcc action enum.

        Trying, // Trying tcc action enum.

        Confirming, // Confirming tcc action enum.

        Canceling, //  Canceling tcc action enum.

        Delete, // DELETE action enum.

        Death, // Death action enum.
    }
}