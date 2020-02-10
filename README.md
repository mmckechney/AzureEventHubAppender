# Introduction

This project is a log4net appender that will write to an Azure Event Hub.  

This Appender has a front end that takes in messages. A blocking collection is used as a buffer between the front end
and the backend.  As the front end taked in messages, they are stored in the blocking collection.  The backend is a thread
that reads from the blocking collection and sends messages to the event hub.  If there are multiple messages available they
will be sent to the event hub.

## Getting Started

1. Add a Nuget reference to [BlueSkyDev.Logging.AzureEventHubAppender](https://www.nuget.org/packages/BlueSkyDev.Logging.AzureEventHubAppender/)

2. For a .NET Framework application add the following log4net configuration to either your log4net.config file or yout app.config file.

```XML
<?xml version="1.0" encoding="utf-8"?>
<log4net>
  <root>
    <level value="INFO"/>
    <appender-ref ref="AzureEventHubAppender"/>
  </root>
 <appender name="AzureEventHubAppender" type="BlueSkyDev.Logging.AzureEventHubAppender, BlueSkyDev.Logging.AzureEventHubAppender">
    <param name="ConnectionString" value="" />
    <param name="BufferSize" value="3000" />
    <param name="BatchSize" value="200" />
    <param name="LogAsJson" value="true" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="date=%utcdate|Thread=%thread|Message=%message|Level=%level|Logger=%logger" />
    </layout>
  </appender>
</log4net>
```

**NOTE:** By default, the logger will produce a JSON output. In order to make sure this works properly, make sure your `conversionPattern` 
value has a delimited format of `"key1=%pattern1|key2=%pattern2|key3=%pattern3"`. This will allow the appender to parse the pattern and generate the JSON that gets sent to the EventHub.

If you want to send the pattern as you have written it, change the `LogAsJson` parameter value to `false`.


You can set your ConnectionString via an Environment Variable named `AzureEventHubAppenderConnectionString`
