namespace AopWrapper.Handlers
{
    /// <summary>
    /// ����ִ��ʧ�ܺ�Ķ���
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