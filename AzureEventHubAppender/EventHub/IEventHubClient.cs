using Microsoft.Azure.EventHubs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueSkyDev.Logging.EventHub
{
    /// <summary>
    /// Interface for using Event Hub client - this is just a thin class to make unit testing possible
    /// </summary>
    public interface IEventHubClient
    {
         //
        // Summary:
        //     Asynchronously sends a batch of event data.
        //
        // Parameters:
        //   eventDataList:
        //     An IEnumerable object containing event data instances.
        //
        // Returns:
        //     Returns System.Threading.Tasks.Task.
        //
        // Exceptions:
        //   T:Microsoft.ServiceBus.Messaging.MessageSizeExceededException:
        //     Thrown if the total serialized size of eventDataList exceeds the allowed size
        //     limit for one event transmission (256k by default).
        //
        // Remarks:
        //     User should make sure the total serialized size of eventDataList should be under
        //     the size limit of one event data transmission, which is 256k by default. Also
        //     note that there will be some overhead to form the batch.
        Task SendAsync(IEnumerable<EventData> eventDataList);

        //
        // Summary:
        //     Sends a cleanup message asynchronously to Service Bus to signal the completion
        //     of the usage of an entity.
        //
        // Returns:
        //     If an exception occurs, this method performs an abort operation on the entity
        //     before throwing the exception.
        //
        // Remarks:
        //     In the event of any exceptions, the method will perform an abort on the entity
        //     before throwing the exception.
        Task CloseAsync();


    }
}
