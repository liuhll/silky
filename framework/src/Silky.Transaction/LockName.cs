namespace Silky.Transaction
{
    public static class LockName
    {
        public const string SelfTccRecovery = "SelfTccRecovery_{0}";

        public const string ParticipantTccRecovery = "ParticipantTccRecovery_{0}_{1}";

        public const string CleanRecovery = "CleanRecovery_{0}";

        public const string CleanRecoveryTransaction = "CleanRecoveryTransaction_{0}";

        public const string PhyDeleted = "PhyDeleted";
    }
}