

using System;

using AopDecorator;
using AopDecorator.Handlers;

namespace TestDecorator.Handlers
{
    public class ExceptionHandler : AopHandler
    {
        public override void OnException(MethodContext context)
        {
            Console.WriteLine(context.FullName + " " + this.GetType().Name + " OnException");
            var mthdName = context.FullName;
            context.WillReturn(false);
            LogHelper.LogInfo(mthdName + " ∑¢…˙“Ï≥££∫" + context.Exception.AllMessage());
        }
    }
}