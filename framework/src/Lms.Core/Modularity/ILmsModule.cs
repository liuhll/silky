namespace Lms.Core.Modularity
{
    public interface ILmsModule
    {
        void Initialize(ApplicationContext applicationContext);
        void Shutdown(ApplicationContext applicationContext);
    }
}