namespace Silky.Lms.Core.Exceptions
{
    public interface IHasErrorCode
    { 
        StatusCode ExceptionCode { get; }
    }
}