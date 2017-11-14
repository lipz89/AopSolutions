

using AopDecorator;
using AopDecorator.Handlers;
using System;

namespace TestDecorator.Handlers
{
    public class LogHandler : AopHandler
    {
        public string Message { get; private set; }

        public LogHandler(string message = null)
        {
            Message = message;
        }
        public override void BeginInvoke(MethodContext context)
        {
            Console.WriteLine(context.FullName + " " + this.GetType().Name + " BeginInvoke");
            var mthdName = context.FullName;
            var msg = string.Empty;
            if (Message.IsNotNullOrEmptyOrWhiteSpace())
            {
                msg = "(" + Message + ")";
            }
            LogHelper.LogInfo(mthdName + msg + "¿ªÊ¼");
        }

        public override void EndInvoke(MethodContext context)
        {
            Console.WriteLine(context.FullName + " " + this.GetType().Name + " EndInvoke");
            var mthdName = context.FullName;
            LogHelper.LogInfo(mthdName + " " + "½áÊø");
        }
    }
}