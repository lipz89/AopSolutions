using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace AopWrapper.Handlers
{
    internal static class HandlerCache
    {
        public static readonly Dictionary<int, IAopHandler> cache = new Dictionary<int, IAopHandler>();

        public static void Add(int key, IAopHandler aspect)
        {
            if (!cache.ContainsKey(key))
            {
                cache.Add(key, aspect);
            }
        }

        public static IAopHandler Get(int key)
        {
            if (cache.ContainsKey(key))
            {
                return cache[key] ?? AopHandler.Empty;
            }
            return AopHandler.Empty;
        }

        public static void WriteLine(string info)
        {
            var dirPath = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/")
                : AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(dirPath, "Logs/AopLogs.txt");
            File.AppendAllText(filePath, string.Format("{0:HH:mm:ss}  -  {1}{2}", DateTime.Now, info, Environment.NewLine));
        }
    }
}
