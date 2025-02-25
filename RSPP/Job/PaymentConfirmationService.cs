﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RSPP.Helpers;
using RSPP.Models.DB;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RSPP.Job
{
    public class PaymentConfirmationService : BackgroundService
    {

       private readonly ILogger<PaymentConfirmationService> _logger;
        BackgroundCheck _backgroundCheck;
        private readonly IServiceScopeFactory _scopeFactory;

        public PaymentConfirmationService(ILogger<PaymentConfirmationService> logger, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _generalLogger = generalLogger;
            _clientFactory = clientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentConfirmationService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("PaymentConfirmationService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                    var dbContext = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RSPPdbContext>();
                    _backgroundCheck = new BackgroundCheck(dbContext);
                    //_backgroundCheck.CheckPayment();

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }

        }



    }
}
