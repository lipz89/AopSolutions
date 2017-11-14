using System;
using System.Collections.Generic;
using System.Linq;

using AopDecorator;
using AopDecorator.Handlers;

namespace TestDecorator.Handlers
{
    public abstract class BaseCacheHandler : AopHandler
    {
        public string CacheKey { get; protected set; }
        public IEnumerable<string> ParameterNames { get; private set; }

        protected Cache CacheHelper;
        protected BaseCacheHandler(string cacheKey)
        {
            this.CacheKey = cacheKey;
            CacheHelper = new Cache();
        }
        protected BaseCacheHandler(string cacheKey, params string[] paramterNames) : this(cacheKey)
        {
            this.ParameterNames = paramterNames;
        }
        protected string GetKeyLevel2(MethodContext context)
        {
            if (context.Parameters.IsNullOrEmpty())
            {
                return string.Empty;
            }

            if (ParameterNames.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var pars = context.Parameters.Where(x => ParameterNames.Contains(x.Name));
            if (pars.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return string.Join(",", pars.Select(x => x.Value.ToString()));
        }
    }

    public class CacheHandler : BaseCacheHandler
    {
        public int ExpireTime { get; set; } = 7200;

        public CacheHandler(string cacheKey) : base(cacheKey)
        {
        }
        public CacheHandler(string cacheKey, params string[] paramterNames) : base(cacheKey, paramterNames)
        {
        }

        public override void BeginInvoke(MethodContext context)
        {
            Console.WriteLine(context.FullName + " " + this.GetType().Name + " BeginInvoke");
            object rtn = null;
            if (ParameterNames.IsNullOrEmpty())
            {
                rtn = CacheHelper.GetObject(CacheKey);
            }
            else
            {
                var keyLv2 = GetKeyLevel2(context);
                if (keyLv2.IsNotNullOrEmptyOrWhiteSpace())
                {
                    rtn = CacheHelper.GetObject(CacheKey, keyLv2);
                }
            }

            if (rtn != null && !context.RealReturnType.IsNullOrVoid() && context.RealReturnType.IsInstanceOfType(rtn))
            {
                context.ReturnValue = rtn;
                context.FlowBehavior = FlowBehavior.Return;
            }
        }

        public override void EndInvoke(MethodContext context)
        {
            Console.WriteLine(context.FullName + " " + this.GetType().Name + " EndInvoke");
            var rtn = context.ReturnValue;
            if (ParameterNames.IsNullOrEmpty())
            {
                CacheHelper.SetObject(CacheKey, rtn, ExpireTime);
            }
            else
            {
                var keyLv2 = GetKeyLevel2(context);
                if (keyLv2.IsNotNullOrEmptyOrWhiteSpace())
                {
                    CacheHelper.SetObject(CacheKey, keyLv2, rtn, ExpireTime);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CacheRemoveHandler : BaseCacheHandler
    {
        public CacheRemoveHandler(string cacheKey) : base(cacheKey)
        {
        }
        public CacheRemoveHandler(string cacheKey, params string[] paramterNames) : base(cacheKey, paramterNames)
        {
        }

        public override void EndInvoke(MethodContext context)
        {
            if (ParameterNames.IsNullOrEmpty())
            {
                CacheHelper.RemoveObject(CacheKey);
            }
            else
            {
                var keyLv2 = GetKeyLevel2(context);
                if (keyLv2.IsNotNullOrEmptyOrWhiteSpace())
                {
                    CacheHelper.RemoveObject(CacheKey, keyLv2);
                }
            }
        }
    }
}
