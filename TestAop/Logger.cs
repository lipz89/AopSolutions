using System;
using AopDecorator;
using AopWrapper;
using KingAOP.Aspects;

namespace TestAop
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class LogAspec : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            Logger.Info("OnEntry: Hello KingAOP:" + args.Method.Name);
        }
        public override void OnException(MethodExecutionArgs args)
        {
            Logger.Info("OnException: Hello KingAOP:" + args.Method.Name);
        }
        public override void OnSuccess(MethodExecutionArgs args)
        {
            Logger.Info("OnSuccess: Hello KingAOP:" + args.Method.Name);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Logger.Info("OnExit: Hello KingAOP:" + args.Method.Name);
        }
    }
    public class LogHandler : AopDecorator.Handlers.AopHandler
    {
        public string Message { get; private set; }

        public LogHandler(string message = null)
        {
            Message = message;
        }
        public override void BeginInvoke(MethodContext context)
        {
            var mthdName = context.FullName;
            var msg = string.Empty;
            if (Message.IsNotNullOrEmptyOrWhiteSpace())
            {
                msg = "(" + Message + ")";
            }
            Logger.Info(mthdName + msg + "开始");
        }

        public override void EndInvoke(MethodContext context)
        {

            var mthdName = context.FullName;
            Logger.Info(mthdName + " " + "结束");
        }
    }
    public class LogWrapperHandler : AopWrapper.Handlers.AopHandler
    {
        //public LogHandler()
        //{

        //}
        //public LogHandler(NameValueCollection collection)
        //{
        //    this.SetProperties(collection);
        //}
        public string Message { get; set; }
        public override void BeginInvoke(AopWrapper.Handlers.MethodContext context)
        {
            //var pars = context.Parameters;
            //foreach (var par in pars)
            //{
            //    Console.WriteLine("\t{0},{1}:{2}", par.Name, par.Type, par.Value);
            //}
            Logger.Info("LogHandler begin:" + Message);
        }

        public override void EndInvoke(AopWrapper.Handlers.MethodContext context)
        {
            Logger.Info("LogHandler end:" + Message);
        }
    }
}