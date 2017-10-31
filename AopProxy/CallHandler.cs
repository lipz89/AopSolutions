using System;
using System.Runtime.Remoting.Messaging;

namespace AopProxy
{
    public abstract class CallHandler : Attribute
    {
        public abstract void Post(IMessage msg, object returnValue);

        public abstract void Pre(IMessage msg);

        public int Order { get; set; }
    }
}