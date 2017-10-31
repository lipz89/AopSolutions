using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AspectX
{
    [DebuggerStepThrough]
    public class WorkerTaskT<T>
    {
        internal WorkerTaskT(WorkContext<T> context, Func<Task<T>> work)
        {
            this.Context = context;
            this.Work = work;
        }

        internal Func<Task<T>> Work { get; set; }

        internal Func<Func<Task<T>>, Task<T>> Chain;
        public WorkContext<T> Context { get; }

        /// <summary>  
        /// 织入一个拦截器  
        /// </summary>  
        /// <param name="newAspectDelegate">一个新的拦截器</param>  
        /// <returns></returns>  
        public WorkerTaskT<T> With(Func<Func<Task<T>>, Task<T>> newAspectDelegate)
        {
            if (this.Chain == null)
            {
                this.Chain = newAspectDelegate;
            }
            else
            {
                Func<Func<Task<T>>, Task<T>> existingChain = this.Chain;
                Func<Func<Task<T>>, Task<T>> callAnother = (work) =>
                        existingChain(() => newAspectDelegate(work));
                this.Chain = callAnother;
            }
            return this;
        }

        public async Task<T> Execute()
        {
            if (this.Chain == null)
            {
                T t = await this.Work();
                this.Context.ReturnValue = t;
                return t;
            }
            else
            {
                return await this.Chain(this.Work);
            }
        }

        //public WorkerTaskT<T> WithLog(string message)
        //{
        //    return this.With(async (work) =>
        //    {
        //        var mthdName = this.Context.FullName;
        //        LogHelper.LogInfo(mthdName + " " + "开始");
        //        if (message.IsNotNullOrEmptyOrWhiteSpace())
        //        {
        //            LogHelper.LogInfo(message);
        //        }

        //        var result = await work();

        //        LogHelper.LogInfo(mthdName + " " + "结束");
        //        return result;
        //    });
        //}

        //public WorkerTaskT<T> WithTryCatch(Action<WorkContext<T>> exceptionAction)
        //{
        //    return this.With(async (work) =>
        //    {
        //        try
        //        {
        //            return await work();
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

        //public WorkerTaskT<T> WithTransaction()
        //{
        //    return this.With(async (work) =>
        //    {
        //        using (var tran = TransactionFactory.CreateNew())
        //        {
        //            var mthdName = this.Context.FullName;
        //            LogHelper.LogInfo(mthdName + " 开始事务");
        //            var result = await work();
        //            tran.Complete();
        //            LogHelper.LogInfo(mthdName + " 结束事务");
        //            return result;
        //        }
        //    });
        //}

        //#region 缓存

        //public WorkerTaskT<T> WithCache(string key, int expire = 7200)
        //{
        //    return this.With(async (work) =>
        //    {
        //        //var mthdName = this.Context.FullName;
        //        //LogHelper.LogInfo(mthdName + " 开始缓存");

        //        if (CacheHelper.IsSet(key))
        //        {
        //            //LogHelper.LogInfo(mthdName + " 获取缓存");
        //            return await Task.FromResult(CacheHelper.GetObject<T>(key));
        //        }

        //        T t = await work();
        //        CacheHelper.SetObject(key, t, expire);
        //        //LogHelper.LogInfo(mthdName + " 保存缓存");
        //        return t;
        //    });
        //}

        //public WorkerTaskT<T> WithRemoveCache(params string[] keys)
        //{
        //    return this.With(async (work) =>
        //    {
        //        foreach (var key in keys)
        //        {
        //            CacheHelper.RemoveObject(key);
        //        }

        //        return await work();
        //    });
        //}

        //public WorkerTaskT<T> WithRemoveCacheByPattern(string pattern)
        //{
        //    return this.With(async (work) =>
        //    {
        //        CacheHelper.RemoveByPattern(pattern);

        //        return await work();
        //    });
        //}


        //public WorkerTaskT<T> WithCacheLevel2(string key, string level2Key, int expire = 7200)
        //{
        //    return this.With(async (work) =>
        //    {
        //        //var mthdName = this.Context.FullName;
        //        //LogHelper.LogInfo(mthdName + " 开始缓存");
        //        if (CacheHelper.IsSet(key, level2Key))
        //        {
        //            //LogHelper.LogInfo(mthdName + " 获取缓存");
        //            return await Task.FromResult(CacheHelper.GetObject<T>(key, level2Key));
        //        }

        //        T t = await work();
        //        CacheHelper.SetObject(key, level2Key, t, expire);
        //        //LogHelper.LogInfo(mthdName + " 保存缓存");
        //        return t;
        //    });
        //}

        //public WorkerTaskT<T> WithRemoveCacheLevel2(string level2Key, params string[] keys)
        //{
        //    return this.With(async (work) =>
        //    {
        //        foreach (var key in keys)
        //        {
        //            CacheHelper.RemoveObject(key, level2Key);
        //        }

        //        return await work();
        //    });
        //}

        //public WorkerTaskT<T> WithRemoveCacheLevel2ByPattern(string level2Key, string pattern)
        //{
        //    return this.With(async (work) =>
        //    {
        //        CacheHelper.RemoveByPattern(pattern, level2Key);

        //        return await work();
        //    });
        //}

        //#endregion
    }
}