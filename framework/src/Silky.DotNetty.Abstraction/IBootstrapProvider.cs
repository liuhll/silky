using System.Security.Cryptography.X509Certificates;
using DotNetty.Transport.Bootstrapping;

namespace Silky.DotNetty.Abstraction;

public interface IBootstrapProvider
{
    Bootstrap CreateClientBootstrap();

    X509Certificate2 GetX509Certificate2();
}