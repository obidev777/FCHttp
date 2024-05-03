using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FCHHttp
{
    public class FCHSession
    {
        public HttpSession Session { get; set; }

        private string API = "http://127.0.0.1:6590/";

        public FCHSession(string proxy= "http://127.0.0.1:6590/")
        {
            API = proxy;
            Session = new HttpSession();
        }

        public FCHResponse Get(string url,Dictionary<string,string> headers=null,bool stream = false,int timeout=100)
        {
            Dictionary<string, object> jsondata = new Dictionary<string, object>();
            jsondata.Add("url", url);
            jsondata.Add("stream", stream);
            if (headers != null)
            {
                jsondata.Add("headers", headers);
            }
            string json = JsonConvert.SerializeObject(jsondata);
            var resp = Session.Post($"{API}GET", json: json) as HttpWebResponse;

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string id = resp.GetString();
                double contentlen = 1;
                double bytesread = 0;
                int t = 0;
                while (bytesread < contentlen)
                {
                    FCHResponse data = FCHResponse.FromJson(Session.GetString($"{API}status/{id}"));
                    if (data.headers.Count > 0)
                    {
                        if (data.headers.TryGetValue("Content-Length", out string vallue))
                        {
                            contentlen = double.Parse(vallue);
                        }
                        data.FSession = this;
                        data.contentlen = contentlen;
                        data.api = API;
                        data.is_stream = stream;
                        return data;
                    }
                    if (t >= timeout) break;
                }
            }
            return null;
        }
    }
}
