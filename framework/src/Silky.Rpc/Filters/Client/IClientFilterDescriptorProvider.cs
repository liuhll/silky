namespace Silky.Rpc.Filters;

public interface IClientFilterDescriptorProvider
{
    FilterDescriptor[] GetClientFilters(string serviceEntryId);
}