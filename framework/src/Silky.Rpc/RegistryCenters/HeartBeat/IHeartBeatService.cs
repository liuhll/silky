using System;
using System.Threading.Tasks;

namespace Silky.Rpc.RegistryCenters.HeartBeat;

public interface IHeartBeatService : IAsyncDisposable
{
    void Start(Func<Task> func);
}