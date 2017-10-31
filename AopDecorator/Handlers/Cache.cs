using System;

namespace AopDecorator.Handlers
{
    public class Cache
    {
        public object GetObject(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public void SetObject(string cacheKey, object rtn, int expireTime)
        {
            throw new NotImplementedException();
        }

        internal object GetObject(string cacheKey, string keyLv2)
        {
            throw new NotImplementedException();
        }

        internal void SetObject(string cacheKey, string keyLv2, object rtn, int expireTime)
        {
            throw new NotImplementedException();
        }

        public void RemoveObject(string cacheKey)
        {
            throw new NotImplementedException();
        }

        internal void RemoveObject(string cacheKey, string keyLv2)
        {
            throw new NotImplementedException();
        }
    }
}