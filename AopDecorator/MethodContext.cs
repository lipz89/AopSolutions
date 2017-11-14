using System;

namespace AopDecorator
{
    public class MethodContext
    {
        /// <summary> ����ִ�еĵ����� </summary>
        public object Executor { get; internal set; }
        /// <summary> ������ </summary>
        public string ClassName
        {
            get { return Executor.GetType().Name; }
        }
        /// <summary> �������� </summary>
        public string MethodName { get; internal set; }
        /// <summary> ����ֵ </summary>
        public object ReturnValue { get; internal set; }
        /// <summary> ����ֵ���� </summary>
        public Type ReturnType { get; internal set; }
        /// <summary> ����ֵ���ͷ�Task </summary>
        public Type RealReturnType
        {
            get { return TaskHelper.UnTask(this.ReturnType); }
        }
        /// <summary> �������� </summary>
        public FlowBehavior FlowBehavior { get; internal set; }
        /// <summary> �����б� </summary>
        public Parameter[] Parameters { get; internal set; }
        /// <summary> �쳣��Ϣ </summary>
        public Exception Exception { get; internal set; }

        /// <summary> �׳��쳣ʱ���쳣��Ϣ </summary>
        public string ThrowMessage { get; internal set; }

        /// <summary> �����ӷ����� </summary>
        public string FullName
        {
            get { return this.ClassName + "." + this.MethodName; }
        }

        public void WillThrow(string throwMessage = null)
        {
            this.ThrowMessage = throwMessage;
            this.FlowBehavior = FlowBehavior.ThrowException;
        }
        public void WillRethrow()
        {
            this.FlowBehavior = FlowBehavior.RethrowException;
        }

        public void WillReturn(object returnValue)
        {
            this.FlowBehavior = FlowBehavior.Return;
            this.ReturnValue = returnValue;
        }

        public Exception ThrowException
        {
            get
            {
                var msg = this.ThrowMessage;
                if (msg.IsNullOrEmptyOrWhiteSpace())
                {
                    msg = FullName + " �����ڲ��������쳣��";
                }
                return new Exception(msg, this.Exception);
            }
        }
    }
}