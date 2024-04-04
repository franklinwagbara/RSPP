using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Helpers.SerilogService.GeneralLogs
{
    public class GeneralLogger
    {
        public IConfiguration Configuration { get; }
        private readonly string loggerDir = "ApiLogs\\";

        public GeneralLogger(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void LogRequest(string message, bool isError, string directory)
        {
            var options = $"{loggerDir}\\{directory}\\events.log";
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(
               options,
                outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}",
                fileSizeLimitBytes: 1_000_000,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();

            if (isError)
            {
                Log.Logger.Error(message);
            }
            else
            {
                Log.Logger.Information(message);
            }
        }
        //public void LogRequest(string message, bool isError)
        //{
        //   var options = Configuration.GetSection(nameof(SerilogConfiguration)).Get<SerilogConfiguration>();
        //   Log.Logger = new LoggerConfiguration()
        //   .MinimumLevel.Debug()
        //   .Enrich.FromLogContext()
        //   .WriteTo.File(
        //      options.accountlogger,
        //       outputTemplate: "{Timestamp:o} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}",
        //       fileSizeLimitBytes: 1_000_000,
        //       rollingInterval: RollingInterval.Day,
        //       rollOnFileSizeLimit: true,
        //       shared: true,
        //       flushToDiskInterval: TimeSpan.FromSeconds(1))
        //   .CreateLogger();

        //    if (isError)
        //    {
        //        Log.Logger.Error(message);
        //    }
        //    else
        //    {
        //        Log.Logger.Information(message);
        //    }
        //}

    }

}
