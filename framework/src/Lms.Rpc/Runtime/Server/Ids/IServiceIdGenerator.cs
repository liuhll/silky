using System.Reflection;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Server.Ids
{
    public interface IServiceIdGenerator : ITransientDependency
    {
        string GenerateServiceId(MethodInfo method);
    }
}