using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AspectX
{
    [DebuggerStepThrough]
    public class WorkerTask
    {
        internal WorkerTask(WorkContext context, Func<Task> work)
        {
            this.Context = context;
            this.Work = work;
        }

        internal Func<Task> Work { get; set; }

        internal Func<Func<Task>, Task> Chain;
        public WorkContext Context { get; }

        /// <summary>  
        /// 织入一个拦截器  
        /// </summary>  
        /// <param name="newAspectDelegate">一个新的拦截器</param>  
        /// <returns></returns>  
        public WorkerTask With(Func<Func<Task>, Task> newAspectDelegate)
        {
            if (this.Chain == null)
            {
                this.Chain = newAspectDelegate;
            }
            else
            {
                Func<Func<Task>, Task> existingChain = this.Chain;
                Func<Func<Task>, Task> callAnother = (work) =>
                        existingChain(() => newAspectDelegate(work));
                this.Chain = callAnother;
            }
            return this;
        }

        public async Task Execute()
        {
            if (this.Chain == null)
            {
                await this.Work();
            }
            else
            {
                await this.Chain(this.Work);
            }
        }

        //public WorkerTask WithLog(string message)
        //{
        //    return this.With(async (work) =>
        //    {
        //        var mthdName = this.Context.FullName;
        //        LogHelper.LogInfo(mthdName + " " + "开始");
        //        if (message.IsNotNullOrEmptyOrWhiteSpace())
        //        {
        //            LogHelper.LogInfo(message);
        //        }

        //        await work();

        //        LogHelper.LogInfo(mthdName + " " + "结束");
        //    });
        //}

        //public WorkerTask WithTryCatch(Action<WorkContext> exceptionAction)
        //{
        //    return this.With(async (work) =>
        //    {
        //        try
        //        {
        //            await work();
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

        //public WorkerTask WithTransaction()
        //{
        //    return this.With(async (work) =>
        //    {
        //        using (var tran = TransactionFactory.CreateNew())
        //        {
        //            var mthdName = this.Context.FullName;
        //            LogHelper.LogInfo(mthdName + " 开始事务");
        //            await work();
        //            tran.Complete();
        //            LogHelper.LogInfo(mthdName + " 结束事务");
        //        }
        //    });
        //}
    }
}