using Serilog;
using Serilog.Events;

namespace Microsoft.Extensions.Hosting
{
    public static class EcomMicroServiceLogging
    {
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}
