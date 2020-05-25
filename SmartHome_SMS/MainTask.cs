using NLog;
using SmartHome_SMS.Modem;
using SmartHome_SMS.SmartDom;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHome_SMS
{
    internal class MainTask
    {
        private static readonly Logger Logger = LogManager.GetLogger("Main Task");

        public MainTask()
        {
            Thread automatThread = new Thread(MainTaskHandler);
            automatThread.Start();
        }

        private async void MainTaskHandler()
        {
            while (true)
            {
                try
                {
                    foreach (SMS sms in (await Program.modem.GetUnreadSMS()))
                    {
                        await Program.modem.SetSMSTag(sms.id, 0);
                        string[] command = Program.RemovePolishDiacritics(sms.content.ToLower()).Split(' ');

                        Logger.Info($"Komenda: {string.Join(", ", command)}");

                        switch (command[0])
                        {
                            case "status":
                                Logger.Info($"Numer {sms.number} zażądał statusu.");
                                await Program.modem.SendSMS(new SMS() { number = sms.number, content = await Status() });
                                break;

                            case "przelacz":

                                Logger.Info($"Numer {sms.number} zażądał przełączenia.");
                                if (int.TryParse(command[1], out int id))
                                {
                                    SmartDomDevice dev = (await Program.smartdom.GetDevices()).Find(x => x.id == id);

                                    if (dev == null)
                                    {
                                        Logger.Error("Brak urządzenia o danym ID.");
                                    }
                                    else
                                    {
                                        Logger.Info($"Przełączanie: {dev}");
                                        await Program.smartdom.Change(dev);
                                        await Program.modem.SendSMS(new SMS()
                                        {
                                            number = sms.number,
                                            content = $"Przełączanie: {dev.name}"
                                        });
                                    }
                                }
                                else
                                {
                                    Logger.Info("Bledne ID");
                                }

                                break;

                            case "wlacz":

                                Logger.Info($"Numer {sms.number} zażądał włączenia.");
                                if (int.TryParse(command[1], out int iid))
                                {
                                    SmartDomDevice dev = (await Program.smartdom.GetDevices()).Find(x => x.id == iid);

                                    if (dev == null)
                                    {
                                        Logger.Error("Brak urządzenia o danym ID.");
                                    }
                                    else
                                    {
                                        Logger.Info($"Włączanie: {dev}");
                                        await Program.smartdom.Change(dev, new SmartDomDeviceData() { state = true });
                                        await Program.modem.SendSMS(new SMS()
                                        {
                                            number = sms.number,
                                            content = $"Włączanie: {dev.name}"
                                        });
                                    }
                                }
                                else
                                {
                                    Logger.Info("Bledne ID");
                                }

                                break;

                            case "wylacz":

                                Logger.Info($"Numer {sms.number} zażądał wyłączenia.");
                                if (int.TryParse(command[1], out int i))
                                {
                                    SmartDomDevice dev = (await Program.smartdom.GetDevices()).Find(x => x.id == i);

                                    if (dev == null)
                                    {
                                        Logger.Error("Brak urządzenia o danym ID.");
                                    }
                                    else
                                    {
                                        Logger.Info($"Wyłączanie: {dev}");
                                        await Program.smartdom.Change(dev, new SmartDomDeviceData() { state = false });
                                        await Program.modem.SendSMS(new SMS()
                                        {
                                            number = sms.number,
                                            content = $"Wyłączanie: {dev.name}"
                                        });
                                    }
                                }
                                else
                                {
                                    Logger.Info("Bledne ID");
                                }

                                break;
                        }
                    }

                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    Logger.Fatal(e);
                    await Task.Delay(10000);
                }
            }
        }

        internal async Task<string> Status()
        {
            string resp = "Status:\n";

            foreach (SmartDomDevice dev in await Program.smartdom.GetDevices())
            {
                resp += $"{dev.id} {dev.name}: {dev.GetStatus()}\n";
            }

            return resp;
        }
    }
}