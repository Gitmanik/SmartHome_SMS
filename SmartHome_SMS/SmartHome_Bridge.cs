﻿using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartHome_SMS.SmartDom
{
    internal class SmartHome_Bridge
    {
        private static readonly Logger Logger = LogManager.GetLogger("SmartHome Bridge");

        private static readonly Uri DEVICES = new Uri("http://127.0.0.1/api/devices.php");
        private static readonly Uri CHANGE = new Uri("http://127.0.0.1/api/change.php");

        private readonly HttpClient HTTP_CLIENT = new HttpClient();

        public async Task<List<SmartDomDevice>> GetDevices()
        {
            return JsonConvert.DeserializeObject<List<SmartDomDevice>>(await HTTP_CLIENT.GetStringAsync(DEVICES));
        }

        public async Task Change(SmartDomDevice dev, SmartDomDeviceData d = null)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "id", dev.device_id },
            };
            if (d != null)
                data.Add("data", JsonConvert.SerializeObject(d));

            Logger.Info("Przełączanie: " + dev);
            using (var request = new HttpRequestMessage()
            {
                RequestUri = CHANGE,
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(data)
            })
            {
                await HTTP_CLIENT.SendAsync(request);
            }
        }
    }

    public class SmartDomDevice
    {
        public string device_id, type, name, data;
        public long last_seen;
        public int id;

        public SmartDomDeviceData DevData => JsonConvert.DeserializeObject<SmartDomDeviceData>(data);

        public override string ToString() => $"[{id} {device_id} {type}: {name}]";

        public string GetStatus()
        {
            switch (type)
            {
                case "TOGGLE":
                    return DevData.state ? "Włączony" : "Wyłączony";

                case "MOMENTARY":
                    return "--";

                default:
                    return "Nie zaimplementowano.";
            }
        }
    }

    public class SmartDomDeviceData
    {
        public bool state;
    }
}