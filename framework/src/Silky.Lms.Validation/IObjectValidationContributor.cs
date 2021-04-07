namespace Silky.Lms.Validation
{
    public interface IObjectValidationContributor
    {
        void AddErrors(ObjectValidationContext context);
    }
}