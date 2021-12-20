namespace Silky.Rpc.Auditing;

public interface IAuditSerializer
{
    string Serialize(object obj);

    T Deserialize<T>(string line);
}