namespace Lms.Rpc.XaResource
{
    public interface IXaResource
    {
        void Commit(XId xId, bool var2);

        void End(XId var1, int var2);

        void Forget(XId var1);

        int GetTransactionTimeout();

        bool IsSameRM(IXaResource var1);

        int Prepare(XId var1);

        void Rollback(XId var1);

        bool SetTransactionTimeout(int var1);

        void Start(XId var1, int var2);
    }
}