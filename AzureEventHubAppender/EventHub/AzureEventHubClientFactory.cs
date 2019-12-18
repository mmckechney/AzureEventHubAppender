using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.EventHub
{
    [ExcludeFromCodeCoverage]

    /// <summary>
    /// EventHub Client Factory for an Azure event hub
    /// </summary>
    /// 
    public class AzureEventHubClientFactory : IEventHubClientFactory
    {
        public IEventHubClient GetEventHubClient(string ConnectionString, string EventHubName)
        {
            return AzureEventHubClient.CreateFromConnectionString(ConnectionString, EventHubName);
        }
    }
}
