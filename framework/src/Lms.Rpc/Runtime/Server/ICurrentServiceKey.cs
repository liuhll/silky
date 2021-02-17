namespace Lms.Rpc.Runtime.Server
{
    public interface ICurrentServiceKey
    {
        string ServiceKey { get; }

        void Change(string seviceKey);
    }
}