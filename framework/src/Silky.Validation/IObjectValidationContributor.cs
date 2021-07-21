namespace Silky.Validation
{
    public interface IObjectValidationContributor
    {
        void AddErrors(ObjectValidationContext context);
    }
}