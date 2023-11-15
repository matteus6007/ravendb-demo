namespace Raven54.Subscriptions.Domain.Options
{
    public class RavenOptions
    {
        public Uri Url { get; set; } = new Uri("http://localhost:8080");
        public string DefaultDatabase { get; set; } = "Mobile";
    }
}
