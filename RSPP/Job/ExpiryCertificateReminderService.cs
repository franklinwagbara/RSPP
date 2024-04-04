using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RSPP.Helpers;
using RSPP.Helpers.SerilogService.GeneralLogs;
using RSPP.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RSPP.Job
{
    //public class ExpiryCertificateReminderService
    //{
    //}

    public class ExpiryCertificateReminderService : BackgroundService
    {

        private readonly ILogger<ExpiryCertificateReminderService> _logger;
        BackgroundCheck _backgroundCheck;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GeneralLogger _generalLogger;
        private readonly IHttpClientFactory _clientFactory;
        public ExpiryCertificateReminderService(ILogger<ExpiryCertificateReminderService> logger, IServiceScopeFactory scopeFactory, GeneralLogger generalLogger, IHttpClientFactory clientFactory)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _generalLogger = generalLogger;
            _clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiryCertificateReminderService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("ExpiryCertificateReminderService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                var dbContext = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RSPPdbContext>();
                _backgroundCheck = new BackgroundCheck(dbContext, _generalLogger, _clientFactory);
                _backgroundCheck.CheckExpiredCertificate();
                await Task.Delay(TimeSpan.FromDays(3), stoppingToken);
            }

        }



    }

}
