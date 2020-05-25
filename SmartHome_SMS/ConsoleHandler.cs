using NLog;
using SmartHome_SMS.Modem;
using SmartHome_SMS.SmartDom;
using System;
using System.Threading;

namespace SmartHome_SMS
{
    public class ConsoleHandler
    {
        private static readonly Logger Logger = LogManager.GetLogger("Konsola");

        public ConsoleHandler()
        {
            Thread consoleThread = new Thread(HandleConsole);
            consoleThread.Start();
        }

        private async void HandleConsole()
        {
            while (true)
            {
                string[] command = Console.ReadLine().Split(' ');
                Logger.Info($"Komenda: {string.Join(", ", command)}");

                switch (command[0].ToLower())
                {
                    case "exit":
                        Environment.Exit(0);
                        break;

                    case "sms":
                        Logger.Info("Wszystkie SMS: \n" + string.Join<SMS>(",\n", (await Program.modem.GetAllSMS()).ToArray()));
                        break;

                    case "unread":
                        Logger.Info("Nieodczytane SMS: \n" + string.Join<SMS>(",\n", (await Program.modem.GetUnreadSMS()).ToArray()));
                        break;

                    case "tag":
                        if (command.Length == 1)
                            Logger.Info("Usage: tag <SMS ID> <TAG>");
                        else
                            Logger.Info("Zmienianie tagu SMS: " + Program.modem.SetSMSTag(int.Parse(command[1]), int.Parse(command[2])));
                        break;

                    case "status":
                        Logger.Info(await Program.automat.Status());
                        break;

                    case "przelacz":
                        if (command.Length == 1)
                            Logger.Info("Usage: przelacz <DEV ID>");
                        else
                        {
                            if (int.TryParse(command[1], out int id))
                            {
                                SmartDomDevice dev = (await Program.smartdom.GetDevices()).Find(x => x.id == id);

                                if (dev == null)
                                {
                                    Logger.Error("Brak urządzenia o danym ID.");
                                    return;
                                }
                                Logger.Info("Przełączanie: " + dev);
                                await Program.smartdom.Change(dev);
                            }
                            else
                            {
                                Logger.Warn("Wymagany INT");
                            }
                        }
                        break;
                }
            }
        }
    }
}