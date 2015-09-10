using System;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using YamlDotNet.Serialization;

namespace Heyman
{
    static class Program
    {
        private const string DefaultConfigName = "config.yaml";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        static int Main(string[] args)
        {
            HandleExceptions();
            PrintWelcome();

            var configPath =
                args.FirstOrDefault()
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultConfigName);

            var config = LoadConfig(configPath);

            try
            {
                using (var heyman = new XmppHeyman(config))
                {
                    heyman.Start();
                    WaitForExit();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return -1;
            }

        }

        private static HeymanConfig LoadConfig(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Logger.Warn("Create default configuration {0}", configPath);
                //create config file with default params
                using (var file = new StreamWriter(configPath))
                {
                    new Serializer().Serialize(file, new HeymanConfig());
                }
            }
            Logger.Info("Load configuration from {0}", configPath);
            //read config
            using (var file = new StreamReader(configPath))
            {
                return new Deserializer().Deserialize<HeymanConfig>(file);
            }
        }

        private static void WaitForExit()
        {
            Logger.Info("Environment.UserInteractive = {0}", Environment.UserInteractive);
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
            else
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }

        private static void HandleExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => Logger.Fatal("Unhandled exception. Sender '{0}'. Args: {1}", sender, eventArgs.ExceptionObject);
        }

        #region PrintWelcome

        private static void PrintWelcome()
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            PrintTitle(Console.Out);
            Console.ForegroundColor = old;
            PrintAbout(Console.Out);
        }

        private static void PrintTitle(TextWriter tw)
        {
            tw.WriteLine(@" _    _ ________     ____  __          _   _ ");
            tw.WriteLine(@"| |  | |  ____\ \   / /  \/  |   /\   | \ | |");
            tw.WriteLine(@"| |__| | |__   \ \_/ /| \  / |  /  \  |  \| |");
            tw.WriteLine(@"|  __  |  __|   \   / | |\/| | / /\ \ | . ` |");
            tw.WriteLine(@"| |  | | |____   | |  | |  | |/ ____ \| |\  |");
            tw.WriteLine(@"|_|  |_|______|  |_|  |_|  |_/_/    \_\_| \_|");

        }

        public static void PrintAbout(TextWriter tw)
        {
            tw.WriteLine("|{0,-90}|", "------------------------------------------------------------------------------------------");
            tw.WriteLine("|{0,-90}|", String.Format("{0} Version {1}", AppInfo.Title, AppInfo.Version));
            tw.WriteLine("|{0,-90}|", AppInfo.Description);
            tw.WriteLine("|{0,-90}|", "");
            tw.WriteLine("|{0,-90}|", String.Format("{0}", AppInfo.CopyrightHolder));
            tw.WriteLine("|{0,-90}|", "------------------------------------------------------------------------------------------");
        }


        #endregion
   
    }
}
