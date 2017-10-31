using System;
using System.Diagnostics;

namespace AspectX
{
    /// <summary>
    /// 一个工作
    /// </summary>
    [DebuggerStepThrough]
    public class Worker
    {
        internal Worker(WorkContext context, Action work)
        {
            this.Context = context;
            this.Work = work;
        }
        internal Action Work { get; }

        internal Action<Action> Chain;
        public WorkContext Context { get; }
        /// <summary>  
        /// 织入一个拦截器  
        /// </summary>  
        /// <param name="newAspectDelegate">一个新的拦截器</param>  
        /// <returns></returns>  
        public Worker With(Action<Action> newAspectDelegate)
        {
            if (this.Chain == null)
            {
                this.Chain = newAspectDelegate;
            }
            else
            {
                Action<Action> existingChain = this.Chain;
                Action<Action> callAnother = (work) => existingChain(() => newAspectDelegate(work));
                this.Chain = callAnother;
            }
            return this;
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        public void Execute()
        {
            if (this.Chain == null)
            {
                this.Work();
            }
            else
            {
                this.Chain(this.Work);
            }
        }

        ///// <summary>
        ///// 织入日志操作，在方法执行前后分别写入日志
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public Worker WithLog(string message)
        //{
        //    return this.With((work) =>
        //    {
        //        var mthdName = this.Context.FullName;
        //        LogHelper.LogInfo(mthdName + " " + "开始");
        //        if (message.IsNotNullOrEmptyOrWhiteSpace())
        //        {
        //            LogHelper.LogInfo(message);
        //        }

        //        work();

        //        LogHelper.LogInfo(mthdName + " " + "结束");
        //    });
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="exceptionAction"></param>
        ///// <returns></returns>
        //public Worker WithTryCatch(Action<WorkContext> exceptionAction)
        //{
        //    return this.With((work) =>
        //    {
        //        try
        //        {
        //            work();
        //        }
        //        catch (Exception ex)
        //        {
        //            this.Context.Exception = ex;
        //            var mthdName = this.Context.FullName;
        //            LogHelper.LogInfo(mthdName + " 发生异常");
        //            exceptionAction(this.Context);
        //            switch (this.Context.FlowBehavior)
        //            {
        //                case FlowBehavior.RethrowException:
        //                    throw;
        //                case FlowBehavior.ThrowException:
        //                    throw this.Context.ThrowException;
        //            }
        //        }
        //    });
        //}

        //public Worker WithTransaction()
        //{
        //    return this.With((work) =>
        //    {
        //        using (var tran = TransactionFactory.CreateNew())
        //        {
        //            work();
        //            tran.Complete();
        //        }
        //    });
        //}
    }
}
