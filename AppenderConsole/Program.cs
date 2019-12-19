using System;
using System.Reflection;
using log4net;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using BlueSkyDev.Logging.EventHub;
using BlueSkyDev.Logging;
using Microsoft.Extensions.Configuration;
using log4net.Layout;
using log4net.Appender;
using log4net.Core;
using System.IO;
using System.Threading;

namespace AppenderConsole
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            var repo = LogManager.CreateRepository("log4net-default-repository");
            log4net.Config.XmlConfigurator.Configure(repo, new FileInfo("log4net.Config"));
            
            for(int i=1;i<7;i++)
            {
                log.Info($"Log {i}");
            }

            LogManager.Flush(10000);
            Thread.Sleep(10000); //wait for all messages to get sent
            System.Environment.Exit(0);
        }
    }
}

