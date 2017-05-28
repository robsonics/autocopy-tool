using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Security;
using log4net;
using Topshelf;

namespace Mediroot.Autocopy
{
    class Program
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(FileAutoCopyService));

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            string dst = ConfigurationManager.AppSettings["dst"];
            string src = ConfigurationManager.AppSettings["src"];

            HostFactory.Run(x =>
            {
                x.StartAutomatically();

                x.UseLog4Net();

                x.EnableServiceRecovery(q => q.RestartService(1));

                x.Service<FileAutoCopyService>(s =>
                {
                    s.ConstructUsing(name => new FileAutoCopyService(log));
                    s.WhenStarted(tc => tc.Start(src, dst));
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Mediroot File Autocopy to copy NFZ ack file from src to dst location.");
                x.SetDisplayName("Mediroot File Autocopy1.1");
                x.SetServiceName("Mediroot_File_Autocopy1.1");
            });   

            //string remoteUnc = @"\\192.168.1.200\publiczny";
            //PinvokeWindowsNetworking.connectToRemote(remoteUnc, @"WORKGROUP\Robert", "malgosia");
            //Directory.CreateDirectory(Path.Combine(remoteUnc, "test"));
            //Console.WriteLine();
            //PinvokeWindowsNetworking.disconnectRemote(remoteUnc);
        }
        
    }
}
