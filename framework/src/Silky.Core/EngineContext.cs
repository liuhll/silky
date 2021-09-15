using System.Runtime.CompilerServices;

namespace Silky.Core
{
    public class EngineContext
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static IEngine Create()
        {
            return Singleton<IEngine>.Instance ?? (Singleton<IEngine>.Instance = new SilkyEngine());
        }

        public static void Replace(IEngine engine)
        {
            Singleton<IEngine>.Instance = engine;
        }

        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    Create();
                }

                return Singleton<IEngine>.Instance;
            }
        }
    }
}