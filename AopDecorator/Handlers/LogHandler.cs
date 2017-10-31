

namespace AopDecorator.Handlers
{
    public class LogHandler : BaseHandler
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
            LogHelper.LogInfo(mthdName + msg + "¿ªÊ¼");
        }

        public override void EndInvoke(MethodContext context)
        {
            var mthdName = context.FullName;
            LogHelper.LogInfo(mthdName + " " + "½áÊø");
        }
    }
}