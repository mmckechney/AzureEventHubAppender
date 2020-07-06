using log4net;
using System.IO;
using System.Reflection;
using System.Threading;
using BlueSkyDev.Logging;
using log4net.Repository.Hierarchy;
using System.Linq;
namespace AppenderConsole
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string newConnection = "";
        static void Main(string[] args)
        {
            var repo = LogManager.CreateRepository("log4net-default-repository");
            log4net.Config.XmlConfigurator.Configure(repo, new FileInfo("log4net.Config"));
            
            for(int i=1;i<7;i++)
            {
                log4net.ThreadContext.Properties["CustomColumn"] = $"Column {i}";
                log.Info($"Log {i}");
            }
            SetEventHubAppenderConnection(newConnection);

            for (int i = 10; i < 20; i++)
            {
                log4net.ThreadContext.Properties["CustomColumn"] = $"Column {i}";
                log.Info($"Log {i}");
            }


            LogManager.Flush(10000);
            Thread.Sleep(10000); //wait for all messages to get sent
            System.Environment.Exit(0);
        }

        internal static void SetEventHubAppenderConnection(string connectionStr)
        {
            if (!string.IsNullOrWhiteSpace(connectionStr))
            {
                Hierarchy hier = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly()) as Hierarchy;
                if (hier != null)
                {
                    var ehAppender = (AzureEventHubAppender)LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().Where(a => a.Name.Contains("AzureEventHubAppender")).FirstOrDefault();

                    if (ehAppender != null)
                    {
                        ehAppender.ConnectionString = connectionStr;
                        ehAppender.ActivateOptions();
                    }
                }
            }
        }
    }
}

