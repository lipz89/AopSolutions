namespace AopDecorator
{
    /// <summary>
    /// ����ִ��ʧ�ܺ�Ķ���
    /// </summary>
    public enum FlowBehavior
    {
        /// <summary> �����쳣 </summary>
        Default,
        /// <summary> �׳�Դ�쳣 </summary>
        RethrowException,
        /// <summary> �׳��µ��쳣 </summary>
        ThrowException,
        /// <summary> ���ؽ������������޷���ֵ����ͬ�ں����쳣�������ѭ���У�����ֹѭ�� </summary>
        Return
    }
}