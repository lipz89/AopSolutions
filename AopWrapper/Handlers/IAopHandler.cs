namespace AopWrapper.Handlers
{
    public interface IAopHandler
    {
        int Order { get; }
        void BeginInvoke(MethodContext context);

        void EndInvoke(MethodContext context);

        void OnException(MethodContext context);
    }
}