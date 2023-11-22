using Microsoft.Extensions.Logging;
using Raven54.Subscriptions.Domain.Models;
using System.Text.Json;

namespace Raven54.Subscriptions.Infrastructure.DocumentProcessors
{
    public class MobileDeviceProcessor : IDocumentProcessor<MobileDevice>
    {
        private readonly ILogger<MobileDeviceProcessor> _logger;

        public MobileDeviceProcessor(ILogger<MobileDeviceProcessor> logger)
        {
            _logger = logger;
        }

        public Task ProcessDocumentAsync(MobileDevice document)
        {
            _logger.LogInformation("'{documentType}' change: {json}", typeof(MobileDevice), JsonSerializer.Serialize(document));

            return Task.CompletedTask;
        }
    }
}
