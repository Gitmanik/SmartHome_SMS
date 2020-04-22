using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartHome_SMS.Modem
{
    internal class ZTE_MF823_Bridge
    {
        private static readonly Logger Logger = LogManager.GetLogger("Most Automat-Modem");

        private static readonly Uri SMS_LIST = new Uri("http://192.168.0.1/goform/goform_get_cmd_process?cmd=sms_data_total&page=0&data_per_page=1000&mem_store=1&tags=10&order_by=order+by+id+desc");
        private const string SMS_SETTAG = "http://192.168.0.1/goform/goform_set_cmd_process?isTest=false&goformId=SET_MSG_READ&msg_id=";
        private static readonly Uri SET_CMD = new Uri("http://192.168.0.1/goform/goform_set_cmd_process");

        private static CultureInfo cultureInfo = CultureInfo.GetCultureInfo("pl-PL");

        private readonly HttpClient HTTP_CLIENT = new HttpClient();

        public async Task<List<SMS>> GetAllSMS()
        {
            List<SMS> l = JsonConvert.DeserializeObject<ZTE_MF823_SMSLIST>(await HTTP_CLIENT.GetStringAsync(SMS_LIST)).messages;
            l.ForEach(sms => sms.Decode());
            return l;
        }

        public async Task<List<SMS>> GetUnreadSMS() => (await GetAllSMS()).FindAll(x => x.tag == 1);

        public async Task SetSMSTag(int id, int newtag) => await HTTP_CLIENT.GetStringAsync(new Uri($"{SMS_SETTAG}{id};&tag={newtag}"));

        public async Task SendSMS(SMS sms)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "isTest", "false" },
                { "goformId", "SEND_SMS" },
                { "notCallback", "true" },
                { "Number", sms.number },
                { "sms_time", new DateTime().ToString("yy;MM;dd;HH;mm;ss", cultureInfo) + "+1" },
                { "MessageBody", sms.EncodeContent() },
                { "ID", "-1" },
                { "encode_type", sms.content.Length > 70 ? "GSM7_default" : "UNICODE" }
            };

            Logger.Info("Wysyłanie: " + sms);
            using (var request = new HttpRequestMessage()
            {
                RequestUri = SET_CMD,
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(data)
            })
            {
                await HTTP_CLIENT.SendAsync(request);
            }
        }
    }

    internal class ZTE_MF823_SMSLIST
    {
        public List<SMS> messages;
    }

    public class SMS
    {
        public override string ToString() => $"[{id}, {tag}: {number} - {content}]";

        public string content;
        public string number;
        public int id;
        public int tag;

        internal string DecodeContent(string c)
        {
            string x = "";
            for (int i = 0; i <= c.Length - 4; i += 4)
            {
                x += (char)int.Parse(c.Substring(i, 4), NumberStyles.HexNumber);
            }
            return x;
        }

        internal void Decode()
        {
            content = DecodeContent(content);
        }

        internal string EncodeContent()
        {
            var haut = 0;
            var result = "";

            string textString = (content.Length > 70) ? Program.RemovePolishDiacritics(content) : content;

            for (int a = 0; a < textString.Length; a++)
            {
                int b = textString[a];
                if (haut != 0)
                {
                    if (0xDC00 <= b && b <= 0xDFFF)
                    {
                        result += (0x10000 + ((haut - 0xD800) << 10) + (b - 0xDC00)).ToString("X2");
                        haut = 0;
                        continue;
                    }
                    else
                    {
                        haut = 0;
                    }
                }
                if (0xD800 <= b && b <= 0xDBFF)
                {
                    haut = b;
                }
                else
                {
                    string cp = b.ToString("X2");
                    while (cp.Length < 4)
                    {
                        cp = '0' + cp;
                    }
                    result += cp;
                }
            }
            return result.ToUpper();
        }
    }
}