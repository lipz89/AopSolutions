using System.Collections.Generic;

namespace TestDecorator.Handlers
{
    public class Cache
    {
        private readonly IDictionary<string, object> cache = new Dictionary<string, object>();
        public object GetObject(string cacheKey)
        {
            if (cache.ContainsKey(cacheKey))
            {
                return cache[cacheKey];
            }

            return null;
        }

        public void SetObject(string cacheKey, object rtn, int expireTime)
        {
            if (cache.ContainsKey(cacheKey))
            {
                cache[cacheKey] = rtn;
            }
            else
            {
                cache.Add(cacheKey, rtn);
            }
        }

        public object GetObject(string cacheKey, string keyLv2)
        {
            if (cache.ContainsKey(cacheKey))
            {
                var dic = cache[cacheKey] as IDictionary<string, object>;
                if (dic != null && dic.ContainsKey(keyLv2))
                {
                    return dic[keyLv2];
                }
            }

            return null;
        }

        public void SetObject(string cacheKey, string keyLv2, object rtn, int expireTime)
        {
            IDictionary<string, object> dic = null;
            if (cache.ContainsKey(cacheKey))
            {
                dic = cache[cacheKey] as IDictionary<string, object>;
            }

            if (dic == null)
            {
                dic = new Dictionary<string, object>();
            }

            if (dic.ContainsKey(keyLv2))
            {
                dic[keyLv2] = rtn;
            }
            else
            {
                dic.Add(keyLv2, rtn);
            }

            SetObject(cacheKey, dic, expireTime);
        }

        public void RemoveObject(string cacheKey)
        {
            if (cache.ContainsKey(cacheKey))
            {
                cache.Remove(cacheKey);
            }
        }

        internal void RemoveObject(string cacheKey, string keyLv2)
        {
            if (cache.ContainsKey(cacheKey))
            {
                var dic = cache[cacheKey] as IDictionary<string, object>;
                if (dic != null && dic.ContainsKey(keyLv2))
                {
                    dic.Remove(keyLv2);
                }
            }
        }
    }
}