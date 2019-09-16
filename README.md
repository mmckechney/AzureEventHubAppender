# Introduction 
This project is a log4net appender that will write to an Azure Event Hub.  

This Appender has a front end that takes in messages. A blocking collection is used as a buffer between the front end
and the backend.  As the front end taked in messages, they are stored in the blocking collection.  The backend is a thread
that reads from the blocking collection and sends messages to the event hub.  If there are multiple messages available they
will be sent to the event hub.


See AzureEventHubAppender.cs for more details.

# Getting Started
Build this porject and reference it in your log4net configuration

