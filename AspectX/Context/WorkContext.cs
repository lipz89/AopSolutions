using System;

namespace AspectX
{
    /// <summary>
    /// ����������
    /// </summary>
    public class WorkContext
    {
        /// <summary> ������ </summary>
        public string ClassName { get; internal set; }

        /// <summary> �������� </summary>
        public string MethodName { get; internal set; }

        /// <summary> ����ֵ���� </summary>
        public Type ReturnType { get; internal set; }

        /// <summary> ����ֵ���ͷ�Task </summary>
        public Type RealReturnType
        {
            get { return TaskHelper.UnTask(this.ReturnType); }
        }

        /// <summary> �������� </summary>
        public FlowBehavior FlowBehavior { get; protected set; }

        /// <summary> �쳣��Ϣ </summary>
        public Exception Exception { get; protected set; }

        /// <summary> �׳��쳣ʱ���쳣��Ϣ </summary>
        public string ThrowMessage { get; protected set; }

        /// <summary> �����ӷ����� </summary>
        public string FullName
        {
            get { return this.ClassName + "." + this.MethodName; }
        }

        public void SetException(Exception exception)
        {
            this.Exception = exception;
        }

        public void WillThrow()
        {
            this.FlowBehavior = FlowBehavior.ThrowException;
        }
        public void WillRethrow()
        {
            this.FlowBehavior = FlowBehavior.RethrowException;
        }

        public Exception ThrowException
        {
            get
            {
                return new Exception(this.ThrowMessage ?? FullName + " �����ڲ��������쳣��", this.Exception);
            }
        }
    }  /// <summary>
       /// ����������
       /// </summary>
    public class WorkContext<T> : WorkContext
    {
        /// <summary> ����ֵ </summary>
        public T ReturnValue { get; set; }

        public void WillReturn(T returnValue = default(T))
        {
            this.FlowBehavior = FlowBehavior.Return;
            this.ReturnValue = returnValue;
        }
    }
}