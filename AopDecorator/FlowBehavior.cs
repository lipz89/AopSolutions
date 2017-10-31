namespace AopDecorator
{
    /// <summary>
    /// 工作执行失败后的动作
    /// </summary>
    public enum FlowBehavior
    {
        /// <summary> 忽略异常 </summary>
        Default,
        /// <summary> 抛出源异常 </summary>
        RethrowException,
        /// <summary> 抛出新的异常 </summary>
        ThrowException,
        /// <summary> 返回结果，如果工作无返回值，等同于忽略异常，如果在循环中，会中止循环 </summary>
        Return
    }
}