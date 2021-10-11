namespace Silky.Lock
{
    public enum DistributedLockType
    {
        SqlServer,

        Postgresql,

        Redis,

        Azure,

        ZooKeeper,

        FileSystem,

        // (Windows only)
        WaitHandles
    }
}