using Raven35.Changes.Subscription.Domain.Models;
using System;
using System.Text.Json;

namespace Raven35.Changes.Subscription.Infrastructure.Observers
{
    public class MobileDeviceObserver : IObserver<MobileDevice>
    {
        public void OnCompleted()
        {
            Console.WriteLine("{0} completed", typeof(MobileDeviceObserver));
        }

        public void OnError(Exception error)
        {
            // TODO: log error
            throw new NotImplementedException();
        }

        public void OnNext(MobileDevice value)
        {
            Console.WriteLine("{0} change: {1}", typeof(MobileDeviceObserver), JsonSerializer.Serialize(value));
        }
    }
}
