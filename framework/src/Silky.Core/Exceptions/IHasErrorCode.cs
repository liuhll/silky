namespace Silky.Core.Exceptions
{
    public interface IHasErrorCode
    { 
        StatusCode ExceptionCode { get; }
    }
}