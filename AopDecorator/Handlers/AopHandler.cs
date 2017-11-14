using System;

namespace AopDecorator.Handlers
{
    public abstract class AopHandler : Attribute, ICallHandler
    {
        public int Order { get; set; }

        public static AopHandler Empty { get { return new EmptyHandler(); } }

        public virtual void BeginInvoke(MethodContext context)
        {
        }

        public virtual void EndInvoke(MethodContext context)
        {
        }

        public virtual void OnException(MethodContext context)
        {
        }
    }
}