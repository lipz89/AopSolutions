using System;

using AopWrapper.Handlers;

namespace Test
{
    public class LogHandler : AopHandler
    {
        //public LogHandler()
        //{

        //}
        //public LogHandler(NameValueCollection collection)
        //{
        //    this.SetProperties(collection);
        //}
        public string Message { get; set; }
        public override void BeginInvoke(MethodContext context)
        {
            //var pars = context.Parameters;
            //foreach (var par in pars)
            //{
            //    Console.WriteLine("\t{0},{1}:{2}", par.Name, par.Type, par.Value);
            //}
            Console.WriteLine("LogHandler begin:" + Message);
        }

        public override void EndInvoke(MethodContext context)
        {
            Console.WriteLine("LogHandler end:" + Message);
        }
    }
}