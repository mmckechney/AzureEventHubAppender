using System.Diagnostics.CodeAnalysis;

namespace BlueSkyDev.Logging.EventHub
{
    [ExcludeFromCodeCoverage]

    /// <summary>
    /// EventHub Client Factory for an Azure event hub
    /// </summary>
    /// 
    public class AzureEventHubClientFactory : IEventHubClientFactory
    {
        public IEventHubClient GetEventHubClient(string ConnectionString)
        {
            return AzureEventHubClient.CreateFromConnectionString(ConnectionString);
        }
    }
}
