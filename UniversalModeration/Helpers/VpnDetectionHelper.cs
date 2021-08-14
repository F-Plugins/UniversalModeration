using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UniversalModeration.Helpers
{
    public class VpnDetectionHelper
    {
        public static async Task<bool> IsVpn(string ip, string apiKey)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("X-Key", apiKey);

            var response = await client.GetAsync(string.Format("http://v2.api.iphub.info/ip/{0}", ip));

            if (response.IsSuccessStatusCode)
            {
                var @object = JObject.Parse(await response.Content.ReadAsStringAsync());

                return (int)@object["block"] == 1;
            }

            return false;
        }
    }
}
