using System;
using System.Runtime.Remoting.Messaging;

namespace AopProxy
{
    public class LogHandler : CallHandler
    {
        public override void Post(IMessage msg, object returnValue)
        {
            Console.WriteLine("post");
        }

        public override void Pre(IMessage msg)
        {
            Console.WriteLine("pre");
        }
    }
}