using System;
using System.Diagnostics;
using NLog;
using SFA.DAS.EAS.DbMaintenance.WebJob.DependencyResolution;
using SFA.DAS.EAS.DbMaintenance.WebJob.Jobs;

namespace SFA.DAS.EAS.DbMaintenance.WebJob
{
    public class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            try
            {
                var container = IoC.Initialize();

                foreach (var job in container.GetAllInstances<IJob>())
                {
                    var jobTypeName = job.GetType().Name;

                    Logger.Info($"Job '{jobTypeName}' started.");
                    job.Run().Wait();
                    Logger.Info($"Job '{jobTypeName}' finished.");
                }

                container.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press enter to close...");
                Console.ReadLine();
            }
        }
    }
}