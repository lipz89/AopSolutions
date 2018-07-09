using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AopProxy;
using Autofac;
using AopDecorator;
using AopWrapper.Aop;

namespace TestAop
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestNoAspect();
            //TestHandwritingAspect();
            //TestAspectX();

            //TestSub();
            //TestDecorator();

            //TestProxy();
            //TesAoptProxy();

            //TestKingAop();

            TestAopDecorator();
            //TestAopWrapper();

            Console.ReadKey();
        }

        private static void TestNoAspect()
        {
            ISimpleService svc = new SimpleService();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }
        private static void TestHandwritingAspect()
        {
            ISimpleService svc = new HandwritingService();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }

        private static void TestAspectX()
        {
            ISimpleService svc = new AspectXService();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }

        private static void TestSub()
        {
            ISimpleService svc = new SubSimpleService();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }

        private static void TestDecorator()
        {
            ISimpleService svc = new SimpleDecoratorService(new SimpleService());
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }
        
        private static void TestProxy()
        {
            ISimpleService svc = TransparentProxy.Create(new ProxyService());
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }
        private static void TesAoptProxy()
        {
            ISimpleService _svc = TransparentProxy.Create(new ProxyService());
            var build = new ContainerBuilder();
            build.Register((c)=> TransparentProxy.Create(new ProxyService())).As<ISimpleService>().InstancePerLifetimeScope();
            var container = build.Build();
            var svc = container.Resolve<ISimpleService>();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }
        private static void TestKingAop()
        {
            ISimpleService svc = new KingAopService();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
        }

        private static void TestAopDecorator()
        {
            ISimpleService svc = Proxy.Of<SimpleService, ISimpleService>();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
            Proxy.Save();
        }

        private static void TestAopWrapper()
        {
            ISimpleService svc = AOPFactory.CreateInstance<SimpleService, ISimpleService>();
            svc.Execute();
            var rst = svc.GetResult();
            Console.WriteLine("执行结果为：" + rst);
            AOPFactory.Save();
        }
    }
}
