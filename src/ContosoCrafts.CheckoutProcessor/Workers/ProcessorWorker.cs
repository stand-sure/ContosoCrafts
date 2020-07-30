using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoCrafts.CheckoutProcessor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContosoCrafts.CheckoutProcessor.Workers
{
    public class ProcessorWorker : BackgroundService
    {
        private readonly ILogger<ProcessorWorker> _logger;
        private readonly RabbitMQBus _messageBus;

        public ProcessorWorker(ILogger<ProcessorWorker> logger, RabbitMQBus messageBus)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running");

            _messageBus.Consume(stoppingToken);

            stoppingToken.WaitHandle.WaitOne();
            _logger.LogInformation("Worker stopped");

            return Task.CompletedTask;
        }
    }
}
