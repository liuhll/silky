using System.Collections.Generic;

namespace LmsTestBase.TestBase.Logging
{
    public interface ICanLogOnObject
    {
        List<string> Logs { get; }
    }
}