# Introduction

This project is a log4net appender that will write to an Azure Event Hub.  

This Appender has a front end that takes in messages. A blocking collection is used as a buffer between the front end
and the backend.  As the front end taked in messages, they are stored in the blocking collection.  The backend is a thread
that reads from the blocking collection and sends messages to the event hub.  If there are multiple messages available they
will be sent to the event hub.


See AzureEventHubAppender.cs for more details.

## Getting Started

1. Add a Nuget reference to AzureEventHubAppender (URL TBD)

2. For a .NET Framework application add the following 

```XML
<?xml version="1.0" encoding="utf-8"?>
<log4net>
  <root>
    <level value="INFO"/>
    <appender-ref ref="AzureEventHubAppender"/>
  </root>
 <appender name="AzureEventHubAppender" type="BlueSkyDev.Logger.AzureEventHubAppender">
    <param name="ConnectionString" value=""/>
    <param name="BufferSize" value="3000"/>
    <param name="BatchSize" value="200"/>
  </appender>
</log4net>
```


