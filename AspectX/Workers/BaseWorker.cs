using System.Diagnostics;

namespace AspectX
{
    [DebuggerStepThrough]

    public abstract class BaseWorker
    {
        public WorkContext Context { get; }
        protected BaseWorker(WorkContext context)
        {
            this.Context = context;
        }
    }
}