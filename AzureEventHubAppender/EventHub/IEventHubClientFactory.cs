﻿namespace BlueSkyDev.Logging.EventHub
{
    /// <summary>
    /// Factory Interface to create an EventHubClient instances
    /// </summary>
    public interface IEventHubClientFactory
    {
        /// <summary>
        /// Get an instance of an EventHubclient
        /// </summary>
        /// <param name="connectionString">Connection string to the event hub namespace</param>
        /// <param name="eventHubName">name of the event hub to connect to</param>
        /// <returns></returns>
        IEventHubClient GetEventHubClient(string connectionString);
    }
}
