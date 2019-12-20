using log4net;
using System.IO;
using System.Reflection;
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
                log4net.ThreadContext.Properties["CustomColumn"] = $"Column {i}";
                log.Info($"Log {i}");
            }

            LogManager.Flush(10000);
            Thread.Sleep(10000); //wait for all messages to get sent
            System.Environment.Exit(0);
        }
    }
}

