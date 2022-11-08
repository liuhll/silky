namespace Silky.Transaction
{
    public static class LockName
    {
        public const string SelfTccRecovery = "SelfTccRecovery_{0}";

        public const string ParticipantTccRecovery = "ParticipantTccRecovery";

        public const string CleanRecovery = "CleanRecovery";

        public const string CleanRecoveryTransaction = "CleanRecoveryTransaction";

        public const string PhyDeleted = "PhyDeleted";
    }
}