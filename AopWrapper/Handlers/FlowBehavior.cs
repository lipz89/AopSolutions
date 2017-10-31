namespace AopWrapper.Handlers
{
    /// <summary>
    /// 工作执行失败后的动作
    /// </summary>
    public enum FlowBehavior
    {
        Default,
        Continue,
        Return,
        ThrowException,
        RethrowException,
        Yield
    }
}