using System;
using System.Threading.Tasks;

namespace Silky.Rpc.RegistryCenters.HeartBeat;

public interface IHeartBeatService : IDisposable
{
    void Start(Func<Task> func);
}