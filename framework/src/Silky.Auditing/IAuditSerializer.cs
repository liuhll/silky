namespace Silky.Auditing;

public interface IAuditSerializer
{
    string Serialize(object obj);
}