namespace Silky.Rpc.Configuration
{
    public interface IRegistryCenterOptions
    {
        string Type { get; }

        bool RegisterSwaggerDoc { get; set; }

    }
}