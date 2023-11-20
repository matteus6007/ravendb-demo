using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
using Raven54.Subscriptions.Domain.Models;
using System.Text.Json;

namespace Raven54.Subscriptions.Infrastructure.Observers
{
    public class DocumentChangeObserver : IObserver<DocumentChange>
    {
        private readonly IDocumentStore _store;

        public DocumentChangeObserver(IDocumentStore store)
        {
            _store = store;
        }

        public void OnCompleted()
        {
            Console.WriteLine("{0} completed", typeof(DocumentChangeObserver));
        }

        public void OnError(Exception error)
        {
            Console.Write(error.ToString());
        }

        public void OnNext(DocumentChange value)
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
                    Console.WriteLine("{0} change: {1}", typeof(DocumentChangeObserver), JsonSerializer.Serialize(mobileDevice));
                }
            }
        }
    }
}
