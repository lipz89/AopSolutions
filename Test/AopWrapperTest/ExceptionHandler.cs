using System;

using AopWrapper.Handlers;

namespace Test
{
    public class ExceptionHandler : AopHandler
    {
        //public ExceptionHandler()
        //{

        //}
        //public ExceptionHandler(NameValueCollection collection)
        //{
        //    this.SetProperties(collection);
        //}
        public override void OnException(MethodContext context)
        {
            Console.WriteLine(context.Exception.AllMessage());
        }
    }
    public static class ExceptionExtensions
    {
        public static string AllMessage(this Exception ex)
        {
            var msg = string.Empty;
            while (ex != null)
            {
                msg += ex.Message + Environment.NewLine;
                ex = ex.InnerException;
            }
            return msg;
        }
    }
}