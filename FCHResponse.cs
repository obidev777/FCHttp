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
    public delegate void OnRead(double readbytes,double totalbytes);

    [Serializable]
    public class FCHResponse
    {
        public string id { get; set; }
        public Dictionary<string,string> headers { get; set; }
        public Dictionary<string,string> cookies { get; set; }
        public string[] content { get; set; }
        public double timespan { get; set; }
        public double time_close { get; set; }
        public int chunks { get; set; }

        public double contentlen = 1;
        public bool is_stream = false;

        public FCHSession FSession;

        internal double bytesread=0;
        internal string api;

        public static FCHResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<FCHResponse>(json);
        }

        public void UpdateData(bool include_content=true)
        {
            FCHResponse data = FCHResponse.FromJson(FSession.Session.GetString($"{api}status/{id}"));
            id = data.id;
            headers = data.headers;
            cookies = data.cookies;
            if(include_content)
                content = data.content;
            timespan = data.timespan;
        }

        public bool WriteToStream(Stream stream,OnRead onRead=null,int bufflen=1024)
        {
            if (this.is_stream) return false;
            int count = 0;
            while (bytesread<contentlen)
            {
                if (bytesread >= contentlen) break;
                if (count == chunks && chunks!=0) break;
                UpdateData();
                if (content.Length > 0)
                {
                    for (int i = 0; i < content.Length; i++)
                    {
                        if (bytesread >= contentlen) break;
                        var time = DateTime.UtcNow - new DateTime(TimeSpan.FromSeconds(timespan).Ticks);
                        if (time.Seconds > (60 * (time_close - 2)))
                        {
                            UpdateData(false);
                        }
                        string chunk = content[i];
                        string host = chunk.Split('/')[0]+"//"+chunk.Split('/')[2];
                        var hs = new HttpSession(cookies: cookies,path:host);
                        var response = hs.Get(chunk) as HttpWebResponse;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            byte[] buffer = new byte[bufflen];
                            using (Stream strm = response.GetResponseStream())
                            {
                                int read = 0;
                                DateTime t = DateTime.Now;
                                while ((read = strm.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    if (bytesread >= contentlen) break;
                                    time = DateTime.UtcNow - new DateTime(TimeSpan.FromSeconds(timespan).Ticks);
                                    if (time.Seconds > (60* (time_close - 2)))
                                    {
                                        UpdateData(false);
                                    }
                                    byte[] readbytes = new byte[read];
                                    Array.Copy(buffer, readbytes, read);
                                    bytesread += read;
                                    if ((DateTime.Now - t).Seconds >= 1)
                                    {
                                        if (onRead != null)
                                        {
                                            onRead(bytesread, contentlen);
                                        }
                                    }
                                    stream.Write(readbytes,0,read);
                                }
                                count++;
                            }
                        }
                        Dictionary<string, string> form = new Dictionary<string, string>();
                        form.Add("url", chunk);
                        FSession.Session.Post($"{api}remove/content/{id}", data: form);
                    }
                }
            }
            if (bytesread >= contentlen)
                return true;
            return false;
        }
    }
}
