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
    }
}
