namespace Silky.Core.Exceptions
{
    public interface IHasErrorCode
    {
        StatusCode StatusCode { get; }

        int Status { get; }
    }
}