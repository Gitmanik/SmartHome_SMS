using NLog;
using NLog.Config;
using NLog.Targets;
using SmartDomSMS.Modem;
using SmartDomSMS.SmartDom;
using System.Text;
using System.Threading;

namespace SmartDomSMS
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetLogger("SmartDomSMS");

        public static ZTE_MF823_Bridge modem;
        public static SmartDom_Bridge smartdom;

        public static Automat automat;
        public static ConsoleHandler console;

        private static void Main()
        {
            ConfigureNLog();

            Logger.Info("SmartDomSMS by Paweł Reich, 2020");

            Logger.Info("Budowanie mostu pomiędzy Automatem a Modemem..");
            modem = new ZTE_MF823_Bridge();

            Logger.Info("Budowanie mostu pomiędzy Automatem a SmartDomem..");
            smartdom = new SmartDom_Bridge();

            Logger.Info("Uruchamianie Automatu..");
            automat = new Automat();

            Logger.Info("Uruchamianie Konsoli..");
            console = new ConsoleHandler();

            Logger.Info("Gotowy");

            new ManualResetEvent(false).WaitOne();
        }

        private static void ConfigureNLog()
        {
            LoggingConfiguration logConfig = new LoggingConfiguration();

            FileTarget logfile = new FileTarget("logfile")
            {
                FileName = "app.log",
                Layout = @"${date:format=HH\:mm\:ss} ${logger:long=True} ${level}: ${message} ${exception}",
                Encoding = Encoding.UTF8
            };

            ConsoleTarget logconsole = new ConsoleTarget("logconsole")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${logger:long=True} ${level}: ${message} ${exception}",
            };

            logConfig.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = logConfig;
        }

        public static string RemovePolishDiacritics(string input)
        {
            string t = "";
            foreach (char c in input.ToCharArray())
            {
                switch (c)
                {
                    case 'ą':
                        t += 'a';
                        break;

                    case 'ć':
                        t += 'c';
                        break;

                    case 'ę':
                        t += 'e';
                        break;

                    case 'ł':
                        t += 'l';
                        break;

                    case 'ń':
                        t += 'n';
                        break;

                    case 'ó':
                        t += 'o';
                        break;

                    case 'ś':
                        t += 's';
                        break;

                    case 'ż':
                    case 'ź':
                        t += 'z';
                        break;

                    case 'Ą':
                        t += 'A';
                        break;

                    case 'Ć':
                        t += 'C';
                        break;

                    case 'Ę':
                        t += 'E';
                        break;

                    case 'Ł':
                        t += 'L';
                        break;

                    case 'Ń':
                        t += 'N';
                        break;

                    case 'Ó':
                        t += 'O';
                        break;

                    case 'Ś':
                        t += 'S';
                        break;

                    case 'Ż':
                    case 'Ź':
                        t += 'Z';
                        break;

                    default:
                        t += c;
                        break;
                }
            }
            return t;
        }
    }
}