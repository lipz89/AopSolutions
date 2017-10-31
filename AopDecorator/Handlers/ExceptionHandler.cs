

namespace AopDecorator.Handlers
{
    public class ExceptionHandler : BaseHandler
    {
        public override void OnException(MethodContext context)
        {
            var mthdName = context.FullName;
            LogHelper.LogInfo(mthdName + " ∑¢…˙“Ï≥££∫" + context.Exception.AllMessage());
        }
    }
}