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

            Console.WriteLine("{0} event: {1}", typeof(DocumentChange), JsonSerializer.Serialize(value));

            using (var session = _store.OpenSession())
            {
                switch (value.CollectionName.ToLowerInvariant())
                {
                    case "mobiledevices":
                            var mobileDevice = session.Load<MobileDevice>(value.Id);

                            if (mobileDevice != null)
                            {
                                Console.WriteLine("{0} change: {1}", typeof(DocumentChangeObserver), JsonSerializer.Serialize(mobileDevice));
                            }

                        break;
                    default:
                        Console.WriteLine("Don't know how to map from collection '{0}'", value.CollectionName);

                        break;
                }
            }
        }
    }
}
