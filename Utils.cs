﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCHHttp
{
    public class Utils
    {
        public static string sizeof_fmt(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = ((double)(len / 1024));
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }
        public static string time_fmt(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);
        }
        public static string progres_bar(double index, double max,string fillbar= "-",string fillprogress= "=", int size= 21,int step_size= 5)
        {
            string bar = "";
            try
            {
                if (max < 1) max += 1;
                var porcent = index / max;
                porcent *= 100;
                porcent = Math.Round(porcent);
                var index_make = 1;
                while (index_make < size)
                {
                    if (porcent >= index_make * step_size)
                        bar += fillprogress;
                    else
                        bar += fillbar;
                    index_make += 1;
                }
            }
            catch { }
               
            return bar;
        }
        public static string GetFileNameFrom(FCHResponse resp)
        {
            if (resp.headers.Count > 0)
            {
                if (resp.headers.TryGetValue("Content-Disposition", out string value))
                {
                    try
                    {
                        return value.Substring(value.IndexOf("filename=")).Replace("filename=", "").Replace("attachment; ", "");
                    }
                    catch { }
                }
            }
            return null;
        }
        public static string GetFileNameFrom(string url)
        {
            try
            {
                string[] tokens = new Uri(url).AbsolutePath.Split('/');
                return tokens[tokens.Length - 1];
            }
            catch { }
            return null;
        }
    }
}
