using Raven54.Subscriptions.Domain.Models;
using System.Text.Json;

namespace Raven54.Subscriptions.Infrastructure.DocumentProcessors
{
    public class MobileDeviceProcessor : IDocumentProcessor<MobileDevice>
    {
        public Task ProcessDocumentAsync(MobileDevice document)
        {
            Console.WriteLine("{0} change: {1}", typeof(MobileDevice), JsonSerializer.Serialize(document));

            return Task.CompletedTask;
        }
    }
}
