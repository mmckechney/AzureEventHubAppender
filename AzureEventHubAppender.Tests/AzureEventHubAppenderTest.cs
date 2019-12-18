using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logging.EventHub;
using System.Diagnostics.CodeAnalysis;
using Moq;
using System.Configuration;
using System.Net;
using log4net.Core;
using Microsoft.Azure.EventHubs;
using log4net.Layout;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Logging;

namespace Logging.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]

    public class AzureEventHubAppenderTest
    {
        [TestMethod]
        public void DefaultConstructorTest()
        {
            AzureEventHubAppender appender = new AzureEventHubAppender();

            Assert.IsNotNull(appender.EventHubClientFactory);

            Assert.AreEqual(typeof(AzureEventHubClientFactory), appender.EventHubClientFactory.GetType());
        }

        [TestMethod]
        public void ParameterConstructorTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            AzureEventHubAppender appender = new AzureEventHubAppender(mockFactory.Object);

            Assert.IsNotNull(appender.EventHubClientFactory);
            Assert.AreEqual(mockFactory.Object.GetType(), appender.EventHubClientFactory.GetType());
            Assert.AreEqual(mockFactory.Object, appender.EventHubClientFactory);
        }

        [TestMethod]
        public void ActivateNormalTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object);

            if (ConfigurationManager.ConnectionStrings.Count < 0)
            {
                Assert.Fail("At least 1 connection stirng must be defined in the App.Config for this test to work");
            }

            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            appender.EventHubNamespaceConnectionStringName = ConnectionStringName;
            appender.EventHubName = "EventHubName";

            Assert.IsNotNull(appender.ConnectionString);

            appender.ActivateOptions();

            Assert.AreEqual(ConfigurationManager.ConnectionStrings[0].ConnectionString, appender.ConnectionString);

            appender.PublicOnClose();
        }

        [TestMethod]
        public void ActivateMissingConnectionStringNameTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            AzureEventHubAppender appender = new AzureEventHubAppender(mockFactory.Object);

            if (ConfigurationManager.ConnectionStrings.Count < 0)
            {
                Assert.Fail("At least 1 connection stirng must be defined in the App.Config for this test to work");
            }

            string ConnectionStringName = String.Empty;
            appender.EventHubNamespaceConnectionStringName = ConnectionStringName;

            Assert.IsNull(appender.ConnectionString);

            Assert.ThrowsException<ArgumentNullException>(() => appender.ActivateOptions());

        }

        [TestMethod]
        public void ActivateMissingEventHubNameTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            AzureEventHubAppender appender = new AzureEventHubAppender(mockFactory.Object);

            if (ConfigurationManager.ConnectionStrings.Count < 0)
            {
                Assert.Fail("At least 1 connection stirng must be defined in the App.Config for this test to work");
            }

            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            appender.EventHubNamespaceConnectionStringName = ConnectionStringName;
            appender.EventHubName = String.Empty;
            appender.EventHubNameAppSettingKey = "NOY FOUND";

            Assert.IsNotNull(appender.ConnectionString);

            Assert.ThrowsException<ArgumentNullException>(() => appender.ActivateOptions());
        }

        [TestMethod]
        public void ActivateMissingEventHubAppSettingKey()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            AzureEventHubAppender appender = new AzureEventHubAppender(mockFactory.Object);

            if (ConfigurationManager.ConnectionStrings.Count < 0)
            {
                Assert.Fail("At least 1 connection stirng must be defined in the App.Config for this test to work");
            }

            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            appender.EventHubNamespaceConnectionStringName = ConnectionStringName;
            appender.EventHubName = String.Empty;
            appender.EventHubNameAppSettingKey = string.Empty;

            Assert.IsNotNull(appender.ConnectionString);

            Assert.ThrowsException<ArgumentNullException>(() => appender.ActivateOptions());
        }

        [TestMethod]
        public void ActivateMissingConnectionStringTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            AzureEventHubAppender appender = new AzureEventHubAppender(mockFactory.Object);

            if (ConfigurationManager.ConnectionStrings.Count < 0)
            {
                Assert.Fail("At least 1 connection stirng must be defined in the App.Config for this test to work");
            }

            string ConnectionStringName = "Not Found";
            appender.EventHubNamespaceConnectionStringName = ConnectionStringName;
            appender.EventHubName = "EventHubName";

            Assert.IsNull(appender.ConnectionString);

            Assert.ThrowsException<ArgumentNullException>(() => appender.ActivateOptions());

        }

        private class TestAzureEventHubAppender : AzureEventHubAppender
        {
            internal TestAzureEventHubAppender(IEventHubClientFactory eventHubClientFactory) : base(eventHubClientFactory) { }

            public bool PublicRequiresLayout => base.RequiresLayout;

            public void PublicOnClose() { base.OnClose(); }

            public string PublicHostName => GetHostName();

            public string PublicGetConfiguredApplicationSetting(string key) => GetConfiguredApplicationSetting(key);
            public string PublicGetConfiguredConnectionString(string key) => GetConfiguredConnectionString(key);
            public string PublicGetConfiguredEventHubName() => GetConfiguredEventHubName();

            public void PublicAppend(LoggingEvent loggingEvent) { base.Append(loggingEvent); }

            public EventData PublicGetEventData(LoggingEvent loggingEvent) { return GetEventData(loggingEvent); }

            public int PublicGetAdditionalItemCount(int available) { return GetAdditionalItemCount(available);  }

        }

        [TestMethod]
        public void RequiresLayoutTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object);

            Assert.IsTrue(appender.PublicRequiresLayout);
        }

        [TestMethod]
        public void OnCloseNormalTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object);

            if (ConfigurationManager.ConnectionStrings.Count < 0)
            {
                Assert.Fail("At least 1 connection stirng must be defined in the App.Config for this test to work");
            }

            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            appender.EventHubNamespaceConnectionStringName = ConnectionStringName;
            appender.EventHubName = "EventHubName";

            Assert.IsNotNull(appender.ConnectionString);

            appender.ActivateOptions();

            appender.PublicOnClose();
        }

        [TestMethod]
        public void OnCloseWithoutStartTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object);

            if (ConfigurationManager.ConnectionStrings.Count < 0)
            {
                Assert.Fail("At least 1 connection string must be defined in the App.Config for this test to work");
            }

            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            appender.EventHubNamespaceConnectionStringName = ConnectionStringName;
            appender.EventHubName = "EventHubName";

            appender.PublicOnClose();
        }

        [TestMethod]
        public void HostNameFromDnsTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object);

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
            string hostname = Dns.GetHostName();

            Assert.AreEqual(hostname, appender.PublicHostName);
        }

        [TestMethod]
        public void HostNameFromWebSiteNameTest()
        {
            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object);

            string name = "TestWebSite";

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", name);

            Assert.AreEqual(name, appender.PublicHostName);

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);

        }


        private LoggingEvent GetLoggingEvent(string message)
        {
            LoggingEventData data = new LoggingEventData()
            {
                Domain = "Domain",
                ExceptionString = null,
                Identity = "Idenity",
                Level = Level.Error,
                LoggerName = "Logger",
                Message = message

            };

            return new LoggingEvent(data);
        }
        [TestMethod]
        public void AppendNormalTest()
        {
            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            string EventHubName = "EventHubName";
            string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            string message = "This is a logging message";

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            Mock<IEventHubClient> mockEventHub = new Mock<IEventHubClient>();

            IEventHubClient evenHub = mockEventHub.Object;

            mockFactory.Setup(c => c.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()))
                       .Callback((string cs, string ehn) => { Assert.AreEqual(connectionString, cs); Assert.AreEqual(EventHubName, ehn); })
                       .Returns(evenHub);

            mockEventHub.Setup(m => m.SendAsync(It.IsAny<IEnumerable<EventData>>()))
                        .Callback((IEnumerable<EventData> d) => { Assert.AreEqual(1, d.Count()); })
                        .Returns(Task.FromResult(0));
            mockEventHub.Setup(m => m.CloseAsync()).Returns(Task.FromResult(0));

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName
            };

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%level:%message"
            };
            patternLayout.ActivateOptions(); appender.Layout = patternLayout;


            appender.ActivateOptions();

            LoggingEvent loggingEvent = GetLoggingEvent(message);
            appender.PublicAppend(loggingEvent);

            appender.PublicOnClose();

            mockFactory.Verify(mock => mock.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockEventHub.Verify(mock => mock.SendAsync(It.IsAny<List<EventData>>()), Times.Once);
            mockEventHub.Verify(mock => mock.CloseAsync(), Times.Once);

        }

        [TestMethod]
        public void AppendMultiEventTest()
        {
            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            string EventHubName = "EventHubName";
            string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            string message = "This is a logging message";

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            Mock<IEventHubClient> mockEventHub = new Mock<IEventHubClient>();

            IEventHubClient evenHub = mockEventHub.Object;

            mockFactory.Setup(c => c.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(evenHub);

            mockEventHub.Setup(m => m.SendAsync(It.IsAny<IEnumerable<EventData>>()))
                        .Returns(async (IEnumerable<EventData> d) => { await Task.Delay(100); });
            mockEventHub.Setup(m => m.CloseAsync()).Returns(Task.FromResult(0));

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName
            };

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%level:%message"
            };
            patternLayout.ActivateOptions(); appender.Layout = patternLayout;


            appender.ActivateOptions();

            LoggingEvent loggingEvent = GetLoggingEvent(message);

            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);
            appender.PublicAppend(loggingEvent);

            appender.PublicOnClose();

            mockFactory.Verify(mock => mock.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockEventHub.Verify(mock => mock.SendAsync(It.IsAny<List<EventData>>()), Times.AtLeast(2));
            mockEventHub.Verify(mock => mock.CloseAsync(), Times.Once);

        }


        [TestMethod]
        public void AppendGracefulExitTest()
        {
            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            string EventHubName = "EventHubName";
            string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            string message = "This is a logging message";

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            Mock<IEventHubClient> mockEventHub = new Mock<IEventHubClient>();

            IEventHubClient evenHub = mockEventHub.Object;

            mockFactory.Setup(c => c.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(evenHub);

            mockEventHub.Setup(m => m.SendAsync(It.IsAny<IEnumerable<EventData>>()))
                        .Returns(async (IEnumerable<EventData> d) => { await Task.Delay(100); });
            mockEventHub.Setup(m => m.CloseAsync()).Returns(Task.FromResult(0));

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName
            };

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%level:%message"
            };
            patternLayout.ActivateOptions(); appender.Layout = patternLayout;


            appender.ActivateOptions();

            LoggingEvent loggingEvent = GetLoggingEvent(message);

            appender.PublicAppend(loggingEvent);
            Thread.Sleep(500);  // Let the sender send the event - so that the call to Take will hit the InvalidOperationException

            appender.PublicOnClose();

            mockFactory.Verify(mock => mock.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockEventHub.Verify(mock => mock.SendAsync(It.IsAny<List<EventData>>()), Times.Once);
            mockEventHub.Verify(mock => mock.CloseAsync(), Times.Once);

        }

        [TestMethod]
        public void OptionalPropertiesTest()
        {
            string ConnectionStringName = "SomeConnectionStringName";
            string EventHubName = "EventHubName";
            int batchSize = 200;
            int bufferSize = 201;

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
           

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName,
                BatchSize = batchSize,
                BufferSize = bufferSize
            };

            Assert.AreEqual(batchSize, appender.BatchSize);
            Assert.AreEqual(bufferSize, appender.BufferSize);

        }
        [TestMethod]
        public void GetAdditionalItemsTest()
        {
            string ConnectionStringName = "SomeConnectionStringName";
            string EventHubName = "EventHubName";
            int batchSize = 200;
            int bufferSize = 201;

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();


            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName,
                BatchSize = batchSize,
                BufferSize = bufferSize
            };

            // Test Available less than batch size
            int available = 20;
            int additionalItems = appender.PublicGetAdditionalItemCount(available);
            Assert.AreEqual(available, additionalItems);

            // Test Available more than batch size
            available = batchSize + 1;
            additionalItems = appender.PublicGetAdditionalItemCount(available);
            Assert.AreEqual(batchSize - 1, additionalItems);

            // Test available the same as batch size -1

            available = batchSize - 1;
            additionalItems = appender.PublicGetAdditionalItemCount(available);
            Assert.AreEqual(batchSize - 1, additionalItems);
        }

        [TestMethod]
        public void EventHubFailsInitializationTest()
        {
            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            string EventHubName = "EventHubName";
            string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            Mock<IEventHubClient> mockEventHub = new Mock<IEventHubClient>();

            IEventHubClient evenHub = mockEventHub.Object;

            mockFactory.Setup(c => c.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()))
                       .Throws(new Exception("Failed"));


            
            mockEventHub.Setup(m => m.SendAsync(It.IsAny<IEnumerable<EventData>>()))
                        .Callback((IEnumerable<EventData> d) => { Assert.AreEqual(1, d.Count()); })
                        .Returns(Task.FromResult(0));
            mockEventHub.Setup(m => m.CloseAsync()).Returns(Task.FromResult(0));

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName
            };

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%level:%message"
            };
            patternLayout.ActivateOptions(); appender.Layout = patternLayout;
            

            appender.ActivateOptions();

            appender.PublicOnClose();

            mockFactory.Verify(mock => mock.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockEventHub.Verify(mock => mock.CloseAsync(), Times.Never);

        }

        [TestMethod]
        public void AppendBatchSendSingleFailureTest()
        {
            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            string EventHubName = "EventHubName";
            string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            string message = "This is a logging message";

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            Mock<IEventHubClient> mockEventHub = new Mock<IEventHubClient>();

            IEventHubClient evenHub = mockEventHub.Object;

            mockFactory.Setup(c => c.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()))
                       .Callback((string cs, string ehn) => { Assert.AreEqual(connectionString, cs); Assert.AreEqual(EventHubName, ehn); })
                       .Returns(evenHub);

            mockEventHub.SetupSequence(m => m.SendAsync(It.IsAny<IEnumerable<EventData>>()))
                        .Throws(new Exception("Sending is not working"))
                        .Returns(Task.FromResult(0));

            mockEventHub.Setup(m => m.CloseAsync()).Returns(Task.FromResult(0));

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName
            };

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%level:%message"
            };
            patternLayout.ActivateOptions(); appender.Layout = patternLayout;


            appender.ActivateOptions();

            LoggingEvent loggingEvent = GetLoggingEvent(message);
            appender.PublicAppend(loggingEvent);

            appender.PublicOnClose();

            mockFactory.Verify(mock => mock.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockEventHub.Verify(mock => mock.SendAsync(It.IsAny<List<EventData>>()), Times.Exactly(2));
            mockEventHub.Verify(mock => mock.CloseAsync(), Times.Once);

        }

        [TestMethod]
        public void AppendBatchSendMultipleFailureTest()
        {
            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            string EventHubName = "EventHubName";
            string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            string message = "This is a logging message";

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            Mock<IEventHubClient> mockEventHub = new Mock<IEventHubClient>();

            IEventHubClient evenHub = mockEventHub.Object;

            mockFactory.Setup(c => c.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()))
                       .Callback((string cs, string ehn) => { Assert.AreEqual(connectionString, cs); Assert.AreEqual(EventHubName, ehn); })
                       .Returns(evenHub);

            mockEventHub.SetupSequence(m => m.SendAsync(It.IsAny<IEnumerable<EventData>>()))
                        .Throws(new Exception("Sending is not working"))
                        .Throws(new Exception("Sending is not working"))
                        .Returns(Task.FromResult(0));

            mockEventHub.Setup(m => m.CloseAsync()).Returns(Task.FromResult(0));

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName
            };

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%level:%message"
            };
            patternLayout.ActivateOptions(); appender.Layout = patternLayout;
            appender.MaxRetries = 1;
            appender.EventHubRetryDelay = TimeSpan.FromMilliseconds(100);

            appender.ActivateOptions();

            LoggingEvent loggingEvent = GetLoggingEvent(message);
            appender.PublicAppend(loggingEvent);

            appender.PublicOnClose();

            mockFactory.Verify(mock => mock.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockEventHub.Verify(mock => mock.SendAsync(It.IsAny<List<EventData>>()), Times.Exactly(2));
            mockEventHub.Verify(mock => mock.CloseAsync(), Times.Once);

        }

        [TestMethod]
        public void DisposeWithNoBufferTest()
        {
            AzureEventHubAppender appender = new AzureEventHubAppender();

            appender.Dispose();
            appender.Dispose();

            Assert.IsNotNull(appender);

        }

        [TestMethod]
        public void DisposeWithBufferTest()
        {
            string ConnectionStringName = ConfigurationManager.ConnectionStrings[0].Name;
            string EventHubName = "EventHubName";
            string connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            string message = "This is a logging message";

            Mock<IEventHubClientFactory> mockFactory = new Mock<IEventHubClientFactory>();
            Mock<IEventHubClient> mockEventHub = new Mock<IEventHubClient>();

            IEventHubClient evenHub = mockEventHub.Object;

            mockFactory.Setup(c => c.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()))
                       .Callback((string cs, string ehn) => { Assert.AreEqual(connectionString, cs); Assert.AreEqual(EventHubName, ehn); })
                       .Returns(evenHub);

            mockEventHub.Setup(m => m.SendAsync(It.IsAny<IEnumerable<EventData>>()))
                        .Callback((IEnumerable<EventData> d) => { Assert.AreEqual(1, d.Count()); })
                        .Returns(Task.FromResult(0));
            mockEventHub.Setup(m => m.CloseAsync()).Returns(Task.FromResult(0));

            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(mockFactory.Object)
            {
                EventHubNamespaceConnectionStringName = ConnectionStringName,
                EventHubName = EventHubName
            };

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "%level:%message"
            };
            patternLayout.ActivateOptions(); appender.Layout = patternLayout;


            appender.ActivateOptions();

            LoggingEvent loggingEvent = GetLoggingEvent(message);
            appender.PublicAppend(loggingEvent);

            appender.PublicOnClose();

            appender.Dispose();

            mockFactory.Verify(mock => mock.GetEventHubClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockEventHub.Verify(mock => mock.SendAsync(It.IsAny<List<EventData>>()), Times.Once);
            mockEventHub.Verify(mock => mock.CloseAsync(), Times.Once);

        }

        [TestMethod]
        public void AppSettingFromEnvironmentTest()
        {
            //CUSTOMCONNSTR_
            string envKey = "EventHubName";
            string eventHubName = "FROM ENVIRONMENT";
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(null);

            appender.EventHubName = "FROM LOG4NET";
            appender.EventHubNameAppSettingKey = envKey;

            Environment.SetEnvironmentVariable("APPSETTING_" + envKey, eventHubName);

            var fromAppender = appender.PublicGetConfiguredApplicationSetting(envKey);

            Assert.AreEqual(eventHubName, fromAppender);

        }


        [TestMethod]
        public void ConnectionStringFromEnvironmentTest()
        {
            //CUSTOMCONNSTR_
            string envKey = "MyConnectionString";
            string eventHubName = "FROM ENVIRONMENT";
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(null);

            appender.EventHubName = "FROM LOG4NET";
            appender.EventHubNameAppSettingKey = envKey;

            Environment.SetEnvironmentVariable("CUSTOMCONNSTR_" + envKey, eventHubName);

            var fromAppender = appender.PublicGetConfiguredConnectionString(envKey);

            Assert.AreEqual(eventHubName, fromAppender);

        }

        [TestMethod]
        public void EventhubNamegFromEnvironmentTest()
        {
            //CUSTOMCONNSTR_
            string envKey = "EventHubName";
            string eventHubName = "FROM ENVIRONMENT";
            TestAzureEventHubAppender appender = new TestAzureEventHubAppender(null);

            appender.EventHubName = "FROM LOG4NET";
            appender.EventHubNameAppSettingKey = envKey;

            Environment.SetEnvironmentVariable("APPSETTING_" + envKey, eventHubName);

            var fromAppender = appender.PublicGetConfiguredEventHubName();

            Assert.AreEqual(eventHubName, fromAppender);

        }

    }


}
