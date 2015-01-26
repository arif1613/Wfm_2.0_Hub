using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace CommonSystemTestLibrary
{
    public class ResponseConverter
    {
        internal static string ConvertResponseToString(WebResponse response)
        {
            var output = string.Empty;

            var responseStream = response.GetResponseStream();
            if (responseStream == null) return null;
            var streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            var read = new Char[256];
            var count = streamReader.Read(read, 0, 256);
            while (count > 0)
            {
                var str = new string(read, 0, count);
                output += str;
                count = streamReader.Read(read, 0, 256);
            }

            return output;
        }

        public static T ConvertResponseToObject<T>(WebResponse response)
        {
            return JsonConvert.DeserializeObject<T>(ConvertResponseToString(response));
        }
    }
}
