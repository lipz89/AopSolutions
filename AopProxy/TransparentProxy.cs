namespace AopProxy
{
    public static class TransparentProxy
    {
        public static T Create<T>(T target)
        {
            XRealProxy<T> realProxy = new XRealProxy<T>(target);
            T transparentProxy = (T)realProxy.GetTransparentProxy();
            return transparentProxy;
        }
    }
}
