using Silky.Rpc.Configuration;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntryGovernance : GovernanceOptions
    {

        public ServiceEntryGovernance() 
        {
        
        }

        public ServiceEntryGovernance(GovernanceOptions governanceOptions)
        {
            ShuntStrategy = governanceOptions.ShuntStrategy;
            TimeoutMillSeconds = governanceOptions.TimeoutMillSeconds;
            EnableCachingInterceptor = governanceOptions.EnableCachingInterceptor;
            EnableCircuitBreaker = governanceOptions.EnableCircuitBreaker;
            ExceptionsAllowedBeforeBreaking = governanceOptions.ExceptionsAllowedBeforeBreaking;
            BreakerSeconds = governanceOptions.BreakerSeconds;
            AddressFuseSleepDurationSeconds = governanceOptions.AddressFuseSleepDurationSeconds;
            UnHealthAddressTimesAllowedBeforeRemoving = governanceOptions.UnHealthAddressTimesAllowedBeforeRemoving;
            RetryIntervalMillSeconds = governanceOptions.RetryIntervalMillSeconds;
            RetryTimes = governanceOptions.RetryTimes;
            MaxConcurrentHandlingCount = governanceOptions.MaxConcurrentHandlingCount;
        }

        public bool ProhibitExtranet { get; set; } = false;

        public bool IsAllowAnonymous { get; set; } = false;
    }
}