using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
using Raven54.Subscriptions.Domain.Models;
using System.Text.Json;

namespace Raven54.Subscriptions.Infrastructure.Observers
{
    public class DocumentChangeObserver : IObserver<DocumentChange>
    {
        private readonly IDocumentStore _store;
        private readonly ILogger<DocumentChangeObserver> _logger;

        public DocumentChangeObserver(
            IDocumentStore store,
            ILogger<DocumentChangeObserver> logger)
        {
            _store = store;
            _logger = logger;
        }

        public void OnCompleted()
        {
            _logger.LogInformation("Observer '{observer}' completed", typeof(DocumentChangeObserver));
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "Error in observer '{observer}'", typeof(DocumentChangeObserver));
        }

        public void OnNext(DocumentChange value)
        {
            if (value.Type == DocumentChangeTypes.Delete)
            {
                _logger.LogInformation("Document {documentId} deleted", value.Id);

                return;
            }

            _logger.LogInformation("{changeType} event: {json}", typeof(DocumentChange), JsonSerializer.Serialize(value));

            using (var session = _store.OpenSession())
            {
                switch (value.CollectionName.ToLowerInvariant())
                {
                    case "mobiledevices":
                            var mobileDevice = session.Load<MobileDevice>(value.Id);

                            if (mobileDevice != null)
                            {
                                _logger.LogInformation("{observer} change: {json}", typeof(DocumentChangeObserver), JsonSerializer.Serialize(mobileDevice));
                            }

                        break;
                    default:
                        _logger.LogInformation("Don't know how to map from collection '{collectionName}'", value.CollectionName);

                        break;
                }
            }
        }
    }
}
