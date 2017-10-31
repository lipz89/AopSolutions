using System;

namespace AopDecorator.Handlers
{
    public abstract class BaseHandler : Attribute, ICallHandler
    {
        public int Order { get; set; }

        public static BaseHandler Empty { get { return new EmptyHandler(); } }

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