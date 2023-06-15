using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RSPP.Job;
using System.IO;

namespace RSPP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            var host = new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .UseStartup<Startup>()
            .Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices(services =>
                {
                    services.AddHostedService<PaymentConfirmationService>();
                    services.AddHostedService<ExpiryCertificateReminderService>();
                }).ConfigureLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddLog4Net("Configurations/log4Net.config");
                });


        //.ConfigureServices(services =>
        //       services.AddHostedService<PaymentConfirmationService>());

        //.UseShutdownTimeout(TimeSpan.FromSeconds(10))

    }
}
