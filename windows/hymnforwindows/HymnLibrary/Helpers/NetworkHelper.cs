using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HymnLibrary.Helpers
{
    internal class NetworkHelper
    {
        public static async Task<string> DownloadstringAsync(string txtFileUrl)
        {
            string jsonString = null;
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36");
                    var stream = await httpClient.GetStreamAsync(txtFileUrl);
                    StreamReader reader = new StreamReader(stream);
                    jsonString = reader.ReadToEnd();

                }
            }
            catch { }

            return jsonString;
        }
    }
}
