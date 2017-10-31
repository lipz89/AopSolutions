namespace AopDecorator.Handlers
{
    public interface ICallHandler
    {
        int Order { get; }
        void BeginInvoke(MethodContext context);

        void EndInvoke(MethodContext context);

        void OnException(MethodContext context);
    }
}