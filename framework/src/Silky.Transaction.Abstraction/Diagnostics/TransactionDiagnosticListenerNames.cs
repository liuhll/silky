namespace Silky.Transaction.Abstraction.Diagnostics
{
    public static class TransactionDiagnosticListenerNames
    {
        private const string TransactionPrefix = "Silky.Transaction.";

        public const string DiagnosticParticipantTransactionListener =
            TransactionPrefix + "DiagnosticParticipantTransactionListener";

        public const string DiagnosticGlobalTransactionListener =
            TransactionPrefix + "DiagnosticGlobalTransactionListener";


        public const string ParticipantBeginHandle = TransactionPrefix + "ParticipantBeginHandle";
        public const string ParticipantEndHandle = TransactionPrefix + "ParticipantEndHandle";

        public const string GlobalPreTryHandle = TransactionPrefix + "GlobalPreTryHandle";
        public const string GlobalTryingHandle = TransactionPrefix + "GlobalTryingHandle";
        public const string GlobalConfirmingHandle = TransactionPrefix + "GlobalConfirmingHandle";
        public const string GlobalConfirmedHandle = TransactionPrefix + "GlobalConfirmedHandle";
        public const string GlobalCancelingHandle = TransactionPrefix + "GlobalCancelingHandle";
        public const string GlobalCanceledHandle = TransactionPrefix + "GlobalCanceledHandle";
    }
}