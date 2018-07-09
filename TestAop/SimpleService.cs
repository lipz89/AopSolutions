using System;
using System.Dynamic;
using AopProxy;
using KingAOP;

namespace TestAop
{
    public class SimpleService : ISimpleService
    {
        [LogHandler]
        [LogWrapperHandler]
        public virtual void Execute()
        {
            Console.WriteLine("执行业务逻辑");
        }

        [LogHandler]
        [LogWrapperHandler]
        public virtual string GetResult()
        {
            Console.WriteLine("执行业务逻辑，并返回结果");
            var rst = "Result";
            return rst;
        }
    }
    public class SubSimpleService : SimpleService
    {
        public override void Execute()
        {
            Logger.Info("开始执行方法：Execute");
            base.Execute();
            Logger.Info("执行方法完成：Execute");
        }

        public override string GetResult()
        {
            Logger.Info("开始执行方法：GetResult");
            Console.WriteLine("执行业务逻辑，并返回结果");
            var rst = base.GetResult();
            Logger.Info("执行方法完成：GetResult");
            return rst;
        }
    }

    public class SimpleDecoratorService : ISimpleService
    {
        private readonly ISimpleService service;

        public SimpleDecoratorService(ISimpleService service)
        {
            this.service = service;
        }

        public void Execute()
        {
            Logger.Info("开始执行方法：Execute");
            service.Execute();
            Logger.Info("执行方法完成：Execute");
        }

        public string GetResult()
        {
            Logger.Info("开始执行方法：GetResult");
            Console.WriteLine("执行业务逻辑，并返回结果");
            var rst = service.GetResult();
            Logger.Info("执行方法完成：GetResult");
            return rst;
        }
    }
    public class ProxyService : MarshalByRefObject, ISimpleService
    {
        [AopProxy.LogHandler]
        public void Execute()
        {
            Console.WriteLine("执行业务逻辑");
        }

        [AopProxy.LogHandler]
        public string GetResult()
        {
            Console.WriteLine("执行业务逻辑，并返回结果");
            var rst = "Result";
            return rst;
        }
    }
    public class HandwritingService : ISimpleService
    {
        public void Execute()
        {
            Logger.Info("开始执行方法：Execute");
            Console.WriteLine("执行业务逻辑");
            Logger.Info("执行方法完成：Execute");
        }

        public string GetResult()
        {
            Logger.Info("开始执行方法：GetResult");
            Console.WriteLine("执行业务逻辑，并返回结果");
            var rst = "Result";
            Logger.Info("执行方法完成：GetResult");
            return rst;
        }
    }

    public class AspectXService : ISimpleService
    {
        public void Execute()
        {
            AspectX.Aspect.Work(() => { Console.WriteLine("执行业务逻辑"); })
                .WithLog(null)
                .WithTryCatch(x => { Console.WriteLine(x.Exception.Message); })
                .WithMonitor()
                .Execute();
        }

        public string GetResult()
        {
            return AspectX.Aspect.Work<string>(() =>
                {
                    Console.WriteLine("执行业务逻辑，并返回结果");
                    var rst = "Result";
                    return rst;
                })
                .WithLog(null)
                .WithTryCatch(x => { Console.WriteLine(x.Exception.Message); })
                .Execute();
        }
    }

    public class KingAopService : IDynamicMetaObjectProvider, ISimpleService
    {
        [LogAspec]
        public virtual void Execute()
        {
            Console.WriteLine("执行业务逻辑");
        }

        [LogAspec]
        public virtual string GetResult()
        {
            Console.WriteLine("执行业务逻辑，并返回结果");
            var rst = "Result";
            return rst;
        }

        public DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            // need for AOP weaving
            return new AspectWeaver(parameter, this);
        }
    }
}
