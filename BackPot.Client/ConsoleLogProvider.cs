using Quartz.Logging;

namespace BackPot.Client
{
    internal class ConsoleLogProvider : ILogProvider
    {
        public Logger GetLogger(string name) => (level, func, exception, parameters) =>
        {
            if (level >= LogLevel.Info && func != null)
            {
                Console.Error.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {func()}", parameters);
            }
            return true;
        };

        public IDisposable OpenNestedContext(string message) => throw new NotImplementedException();
        public IDisposable OpenMappedContext(string key, object value, bool destructure = false) => throw new NotImplementedException();
    }
}