using Raven.Abstractions.Data;
using Raven.Client;
using Raven35.Changes.Subscription.Domain.Models;
using System;
using System.Text.Json;

namespace Raven35.Changes.Subscription.Infrastructure.Observers
{
    public class DocumentChangeNotificationObserver : IObserver<DocumentChangeNotification>
    {
        private readonly IDocumentStore _store;

        public DocumentChangeNotificationObserver(IDocumentStore store)
        {
            _store = store;
        }

        public void OnCompleted()
        {
            Console.WriteLine("{0} completed", typeof(DocumentChangeNotificationObserver));
        }

        public void OnError(Exception error)
        {
            // TODO: log error
            throw new NotImplementedException();
        }

        public void OnNext(DocumentChangeNotification value)
        {
            Console.WriteLine("{0} on document {1}", value.Type, value.Id);

            using (var session = _store.OpenSession())
            {
                // TODO: work out which object to serialize
                var mobileDevice = session.Load<MobileDevice>(value.Id);

                if (mobileDevice != null)
                {
                    Console.WriteLine("{0} change: {1}", typeof(DocumentChangeNotificationObserver), JsonSerializer.Serialize(mobileDevice));
                }
            }
        }
    }
}
