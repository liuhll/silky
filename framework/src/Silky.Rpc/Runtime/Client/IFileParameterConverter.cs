using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client;

public interface IFileParameterConverter: IScopedDependency
{
    object[] Convert(object[] parameters);
    
    IDictionary<string, object> Convert(IDictionary<string, object> parameters);
    
    IDictionary<ParameterFrom, object> Convert(IDictionary<ParameterFrom, object> parameters);
}