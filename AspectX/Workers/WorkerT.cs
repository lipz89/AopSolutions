using System;
using System.Diagnostics;

namespace AspectX
{
    /// <summary>
    /// 一个有返回值的工作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerStepThrough]
    public class Worker<T>
    {
        internal Worker(WorkContext<T> context, Func<T> work)
        {
            this.Work = work;
            this.Context = context;
        }
        /// <summary>  
        /// Chain of aspects to invoke  
        /// </summary>  
        internal Func<Func<T>, T> Chain;

        public WorkContext<T> Context { get; }

        public Func<T> Work { get; }

        /// <summary>  
        /// 织入一个拦截器
        /// </summary>  
        /// <param name="newAspectDelegate">一个新的拦截器</param>  
        /// <returns></returns>  
        public Worker<T> With(Func<Func<T>, T> newAspectDelegate)
        {
            if (this.Chain == null)
            {
                this.Chain = newAspectDelegate;
            }
            else
            {
                Func<Func<T>, T> existingChain = this.Chain;
                Func<Func<T>, T> callAnother = (work) =>
                    existingChain(() => newAspectDelegate(work));
                this.Chain = callAnother;
            }
            return this;
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <returns></returns>
        public T Execute()
        {
            if (this.Chain == null)
            {
                T t = this.Work();
                this.Context.ReturnValue = t;
                return t;
            }
            else
            {
                return this.Chain(this.Work);
            }
        }
        //public Worker<T> WithLog(string message)
        //{
        //    return this.With((work) =>
        //    {
        //        var mthdName = this.Context.FullName;
        //        LogHelper.LogInfo(mthdName + " " + "开始");
        //        if (message.IsNotNullOrEmptyOrWhiteSpace())
        //        {
        //            LogHelper.LogInfo(message);
        //        }

        //        var result = work();

        //        LogHelper.LogInfo(mthdName + " " + "结束");
        //        return result;
        //    });
        //}

        //public Worker<T> WithTryCatch(Action<WorkContext<T>> exceptionAction)
        //{
        //    return this.With((work) =>
        //    {
        //        try
        //        {
        //            return work();
        //        }
        //        catch (Exception ex)
        //        {
        //            this.Context.Exception = ex;
        //            var mthdName = this.Context.FullName;
        //            LogHelper.LogInfo(mthdName + " 发生异常");
        //            exceptionAction(this.Context);
        //            switch (this.Context.FlowBehavior)
        //            {
        //                case FlowBehavior.Return:
        //                    if (this.Context.ReturnValue != null)
        //                    {
        //                        return this.Context.ReturnValue;
        //                    }
        //                    throw;
        //                case FlowBehavior.RethrowException:
        //                    throw;
        //                case FlowBehavior.ThrowException:
        //                    throw this.Context.ThrowException;
        //            }
        //            return default(T);
        //        }
        //    });
        //}

        //public Worker<T> WithTransaction()
        //{
        //    return this.With((work) =>
        //    {
        //        using (var tran = TransactionFactory.CreateNew())
        //        {
        //            //var mthdName = this.Context.FullName;
        //            //LogHelper.LogInfo(mthdName + " 开始事务");
        //            var result = work();
        //            tran.Complete();
        //            //LogHelper.LogInfo(mthdName + " 结束事务");
        //            return result;
        //        }
        //    });
        //}

        //public Worker<T> WithCache(string key, int expire = 7200)
        //{
        //    return this.With((work) =>
        //    {
        //        //var mthdName = this.Context.FullName;
        //        //LogHelper.LogInfo(mthdName + " 开始缓存");

        //        if (CacheHelper.IsSet(key))
        //        {
        //            //LogHelper.LogInfo(mthdName + " 获取缓存");
        //            return CacheHelper.GetObject<T>(key);
        //        }

        //        T t = work();
        //        CacheHelper.SetObject(key, t, expire);
        //        //LogHelper.LogInfo(mthdName + " 保存缓存");
        //        return t;
        //    });
        //}

        //public Worker<T> WithRemoveCache(params string[] keys)
        //{
        //    return this.With((work) =>
        //    {
        //        foreach (var key in keys)
        //        {
        //            CacheHelper.RemoveObject(key);
        //        }

        //        return work();
        //    });
        //}
        //public Worker<T> WithRemoveCacheByPattern(string pattern)
        //{
        //    return this.With((work) =>
        //    {
        //        CacheHelper.RemoveByPattern(pattern);

        //        return work();
        //    });
        //}

        //public Worker<T> WithCacheLevel2(string key, string level2Key, int expire = 7200)
        //{
        //    return this.With((work) =>
        //    {
        //        //var mthdName = this.Context.FullName;
        //        //LogHelper.LogInfo(mthdName + " 开始缓存");
        //        if (CacheHelper.IsSet(key, level2Key))
        //        {
        //            //LogHelper.LogInfo(mthdName + " 获取缓存");
        //            return CacheHelper.GetObject<T>(key, level2Key);
        //        }

        //        T t = work();
        //        CacheHelper.SetObject(key, level2Key, t, expire);
        //        //LogHelper.LogInfo(mthdName + " 保存缓存");
        //        return t;
        //    });
        //}

        //public Worker<T> WithRemoveCacheLevel2(string level2Key, params string[] keys)
        //{
        //    return this.With((work) =>
        //    {
        //        foreach (var key in keys)
        //        {
        //            CacheHelper.RemoveObject(key, level2Key);
        //        }

        //        return work();
        //    });
        //}
        //public Worker<T> WithRemoveCacheLevel2ByPattern(string level2Key, string pattern)
        //{
        //    return this.With((work) =>
        //    {
        //        CacheHelper.RemoveByPattern(pattern, level2Key);

        //        return work();
        //    });
        //}
    }
}
