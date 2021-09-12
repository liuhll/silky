namespace Silky.Transaction.Tcc.Diagnostics
{
    public static class TccDiagnosticListenerNames
    {
        private const string TccPrefix = "Silky.Transaction.Tcc.";

        private const string DiagnosticTccListenerName = " DiagnosticTccListener";

        public const string ParticipantPreTry = TccPrefix + "PreTryHandle";
        public const string ParticipantTrying = TccPrefix + "ParticipantTrying";

        public const string ParticipantConfirming = TccPrefix + "ParticipantConfirming";
        public const string ParticipantCanceling = TccPrefix + "ParticipantCanceling";
        public const string ParticipantConfirmed = TccPrefix + "ParticipantConfirmed";
        public const string ParticipantCanceled = TccPrefix + "ParticipantCanceled";


        public const string GlobalPreTry = TccPrefix + "GlobalPreTry";
        public const string GlobalTrying = TccPrefix + "GlobalTrying";
        public const string GlobalConfirming = TccPrefix + "GlobalConfirming";
        public const string GlobalCanceling = TccPrefix + "GlobalCanceling";
        public const string GlobalConfirmed = TccPrefix + "GlobalConfirmed";
        public const string GlobalCanceled = TccPrefix + "GlobalCanceled";
    }
}