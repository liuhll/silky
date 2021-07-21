using System.Collections.Generic;

namespace SilkyTestBase.TestBase.Logging
{
    public interface ICanLogOnObject
    {
        List<string> Logs { get; }
    }
}