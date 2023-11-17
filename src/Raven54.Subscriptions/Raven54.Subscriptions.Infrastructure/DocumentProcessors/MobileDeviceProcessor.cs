using Raven54.Subscriptions.Domain.Models;
using System.Text.Json;

namespace Raven54.Subscriptions.Infrastructure.DocumentProcessors
{
    public class MobileDeviceProcessor : IDocumentProcessor<MobileDevice>
    {
        public async Task ProcessDocumentAsync(MobileDevice document)
        {
            await Task.CompletedTask;

            Console.WriteLine("{0} change: {1}", typeof(MobileDevice), JsonSerializer.Serialize(document));
        }
    }
}
