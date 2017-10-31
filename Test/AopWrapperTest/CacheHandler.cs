using System;
using System.Threading.Tasks;

using AopWrapper.Handlers;

namespace Test
{
    public class CacheHandler : AopHandler
    {
        //public CacheHandler()
        //{

        //}
        //public CacheHandler(NameValueCollection collection)
        //{
        //    this.SetProperties(collection);
        //}
        public string CacheKey { get; set; }

        public int DurationMinutes { get; set; }

        public override void BeginInvoke(MethodContext context)
        {

            Console.WriteLine("CacheHandler.BeginInvoke:" + CacheKey);
        }

        public override void EndInvoke(MethodContext context)
        {
            //context.DoWithResult(x =>
            //{
            var x = context.ReturnValue;
            Console.WriteLine(x);
            Console.WriteLine("CacheHandler.EndInvoke:" + CacheKey + "--" + x);
            //});
        }
    }
}
