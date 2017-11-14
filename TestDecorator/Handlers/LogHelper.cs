using System.Diagnostics;

namespace TestDecorator.Handlers
{
    public static class LogHelper
    {
        public static void LogInfo(string s)
        {
            Debug.WriteLine(s);
        }
    }
}