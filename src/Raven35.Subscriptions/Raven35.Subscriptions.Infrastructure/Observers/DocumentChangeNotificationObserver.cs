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
            Console.Write(error.ToString());
        }

        public void OnNext(DocumentChangeNotification value)
        {
            if (value.Type == DocumentChangeTypes.Delete)
            {
                Console.WriteLine("Document {0} deleted", value.Id);

                return;
            }

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
