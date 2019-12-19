using BlueSkyDev.Logging.EventHub;
using log4net.Appender;
using log4net.Core;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Web.Configuration;

namespace BlueSkyDev.Logging
{
    /// <summary>
    /// Appender for sending log data to a Azure EventHub
    /// Incoming log messages are stored in a buffer - and then a background task (sender task) will 
    /// send the log event to the event hub.
    ///  
    /// This will allow the application to add log messages and not impcact performance.  If the buffer is bull the call to log will block
    /// and performance will be impacted.
    /// 
    /// To set this up - the following parameters can be specified in the appender setup
    /// 
    ///     Name - is the type of the of the appender to use StratusLogging is the name space - AzureEventHubAppender is the name of the class
    ///            , StratusLogging is the name of the DLL that holds the type
    ///            
    ///     EventHubNamespaceConnectionStringName - is the name of the connection string (from the connection string section in the config file) - this is a connection string to the event hub namespace
    ///     EvenHubName - is the name of the event hub to send the log events to
    ///     BufferSize - is the size of the buffer - this is optionsl - the default value is 100
    ///     BatchSize - is the maximum number of log event to send at any given time - this is optional - the defaultvalue is 10
    ///     
    ///     <appender name="AzureEventHubAppender" type="StratusLogging.AzureEventHubAppender, StratusLogging">
    ///        <param name = "EventHubNamespaceConnectionStringName" value="EventHubNamespaceConnectionString"/>
    ///        <param name = "EventHubName" value="stratus-poc"/>
    ///        <param name = "EventHubNameAppSettingKey" value="EventHub"/>
    ///        <param name = "BufferSize" value="100"/>
    ///        <param name = "BatchSize" value="10"/>
    ///        <layout type = "log4net.Layout.PatternLayout" >
    ///           < conversionPattern value="------------------------%newline%utcdate%newlineThread=%thread%newlineLevel=%level%newlineLogger=%logger%newlineProperty1=%property{Property1}%newlineProperty2=%property{Property2}%newlineBegin Message...%message%newline" />
    ///        </layout>
    ///     </appender>
    /// 
    /// </summary>
    public class AzureEventHubAppender : AppenderSkeleton, IDisposable
    {
        #region Constructor 

        /// <summary>
        /// Default constructor - use the default IEventHubClientFactory (AzureEventHubClientFactory)
        /// </summary>
        public AzureEventHubAppender()
        {
            EventHubClientFactory = new AzureEventHubClientFactory();
        }

        /// <summary>
        /// Testing Constructor - use provided IEventHubClientFactory
        /// </summary>
        /// <param name="eventHubClientFactory"></param>
        public AzureEventHubAppender(IEventHubClientFactory eventHubClientFactory)
        {
            EventHubClientFactory = eventHubClientFactory;
        }

        #endregion

        #region IDisposable

        // Flag: Has Dispose already been called?
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                }
            }

            disposed = true;
        }
        #endregion

        #region Appender Properties
        /// <summary>
        /// Name of the connection string to the EventHub Namespace
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Size of the internal buffer, this holds the log messages waiting to be sent to the EventHub
        /// </summary>
        public int BufferSize { get; set; } = 100;
        /// <summary>
        /// Maximum number of log messages to send to the EventHub at one time
        /// </summary>
        public int BatchSize { get; set; } = 10;

        /// <summary>
        /// Maximum number of time to retry sending a batch to the event hub
        /// </summary>
        public int MaxRetries { get; set; } = 100;


        public TimeSpan EventHubRetryDelay { get; set; } = TimeSpan.FromMilliseconds(1500);
        #endregion

        #region AppenderSkeleton Overrides

        /// <summary>
        /// Initialize the appended - start the Sending Task (background)
        /// </summary>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (String.IsNullOrEmpty(ConnectionString))
            {
                string message = "ConnectionString is null or empty";
                ErrorHandler.Error(message);
                throw new ArgumentNullException(message);
            }

            StartSendingTask();
        }

        protected override bool RequiresLayout => true;

        /// <summary>
        /// Shutdown this appender - stop the sending task
        /// </summary>
        protected override void OnClose()
        {
            // Stop the sending task
            Buffer.CompleteAdding();

            if (SendingTask != null)
            {
                SendingTask.Wait();
            }

            SendingTask = null;

            base.OnClose();
        }

        /// <summary>
        /// Send a logging event to the event Hub - at this point - add it to the buffer, the sending tasks will do the heavy lifting
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            EventData eventData = GetEventData(loggingEvent);
            Buffer.Add(eventData);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get EventData for a Logging Event
        /// </summary>
        /// <param name="loggingEvent">the logging event to use</param>
        /// <returns>EventData for the specificed LoggingEvent</returns>
        protected EventData GetEventData(LoggingEvent loggingEvent)
        {
            //string content = RenderLoggingEvent(loggingEvent);
            string content = RenderLoggingEventToJson(loggingEvent);
            string message = JsonConvert.SerializeObject(new { data = content, host = GetHostName() }, Formatting.Indented);
            EventData eventData = new EventData(Encoding.UTF8.GetBytes(message));
            return eventData;
        }

        /// <summary>
        /// Sending task - this reads EventData from the buffer - gethers up a batch (if avaialable) and send to the event hub
        /// </summary>
        /// <returns></returns>
        protected string RenderLoggingEventToJson(LoggingEvent loggingEvent)
        {
            var data = loggingEvent.GetLoggingEventData();
            string eventJson = JsonConvert.SerializeObject(data, Formatting.Indented);
            return eventJson;
        }
        private async Task Sender()
        {
            var connectionString = this.ConnectionString;
            var eventHubName = this.EventHubName;
            try
            {
                // Open session with the Event Hub.
                //
                IEventHubClient eventHubClient = EventHubClientFactory.GetEventHubClient(connectionString);

                while (!Buffer.IsCompleted)  // run while there is data avaialble
                {
                    EventData item = null;

                    try
                    {
                        item = Buffer.Take();  // Get the first available item - this will block until there is data 
                    }
                    catch (InvalidOperationException)
                    {
                        continue;  // Nothing left to do - this task is done - clean up and leave
                    }

                    int additional = GetAdditionalItemCount(Buffer.Count);

                    // Build the batch
                    List<EventData> batch = new List<EventData> { item };

                    while (additional > 0)
                    {
                        batch.Add(Buffer.Take());
                        additional--;
                    }

                    bool batchSent = false;
                    int attempt = 0;                    

                    while (!batchSent)
                    {
                        try
                        {
                            attempt++;
                            // Send the batch
                            await eventHubClient.SendAsync(batch).ConfigureAwait(false);
                            batchSent = true;
                        }
                        catch (Exception exception)
                        {
                            string message = $"Exception while sending Batch to EventHub({EventHubName})";
                            Trace.TraceError(message + " - "  + exception.ToString());
                            ErrorHandler.Error(message, exception);

                            if (attempt > MaxRetries)
                            {
                                Trace.TraceError($"Tried {attempt} time to send a batch to Event Hub({EventHubName}) - giving up");
                                ErrorHandler.Error($"Tried {attempt} time to send a batch to Event Hub({EventHubName}) - giving up");
                                batchSent = true;
                            }
                            else
                            {
                                await Task.Delay(EventHubRetryDelay).ConfigureAwait(false);
                            }
                        }
                    }

                    // Not really an error - but debugging information so see how things are working (or not).
                    //
                    ErrorHandler.Error($"Sent batch of size {batch.Count}");
                }

                await eventHubClient.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                string message = $"There was a problem with the EventHub({EventHubName})";
                Trace.TraceError(message);
                ErrorHandler.Error(message, exception);
            }
        }

        protected int GetAdditionalItemCount(int available)
        {
            return Math.Min(available, BatchSize - 1);
        }

        #endregion

        #region Properties
        public string EventHubName
        {
            get
            {
                return Regex.Match(this.ConnectionString, "sb://.*/").Value.Replace("sb://", "").Replace("/", "");
            }
        }
        /// <summary>
        /// Factory use to create EventHubClient objects
        /// </summary>
        public IEventHubClientFactory EventHubClientFactory { get; }

        /// <summary>
        /// Buffer - holds the data to be sent by the Event Hub
        /// </summary>
        private BlockingCollection<EventData> buffer = null;
        private BlockingCollection<EventData> Buffer
        {
            get
            {
                if (buffer == null)
                {
                    buffer = new BlockingCollection<EventData>(BufferSize);
                }

                return buffer;
            }
        }

        /// <summary>
        /// Background task reading from the buffer and sending to the event hub
        /// </summary>
        private Task SendingTask { get; set; }

        /// <summary>
        /// Start the sending task if it is not already running
        /// </summary>
        private void StartSendingTask()
        {
            // Get the Sender going - if not done already
            if (SendingTask == null)
            {
                SendingTask = Task.Run(() => Sender());
            }
        }


        protected string GetHostName()
        {
            string webSiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"); // this is the name of the App Service
            if (String.IsNullOrEmpty(webSiteName))
            {
                return Dns.GetHostName();
            }
            else
            {
                return webSiteName;
            }
        }





        #endregion
    }
}