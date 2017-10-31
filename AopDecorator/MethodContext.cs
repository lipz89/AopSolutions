using System;

namespace AopDecorator
{
    public class MethodContext
    {
        /// <summary> 方法执行的调用者 </summary>
        public object Executor { get; internal set; }
        /// <summary> 类名称 </summary>
        public string ClassName { get; internal set; }
        /// <summary> 方法名称 </summary>
        public string MethodName { get; internal set; }
        /// <summary> 返回值 </summary>
        public object ReturnValue { get; internal set; }
        /// <summary> 返回值类型 </summary>
        public Type ReturnType { get; internal set; }
        /// <summary> 返回值类型非Task </summary>
        public Type RealReturnType
        {
            get { return TaskHelper.UnTask(this.ReturnType); }
        }
        /// <summary> 后续动作 </summary>
        public FlowBehavior FlowBehavior { get; internal set; }
        /// <summary> 参数列表 </summary>
        public Parameter[] Parameters { get; internal set; }
        /// <summary> 异常信息 </summary>
        public Exception Exception { get; internal set; }

        /// <summary> 抛出异常时的异常信息 </summary>
        public string ThrowMessage { get; internal set; }

        /// <summary> 类名加方法名 </summary>
        public string FullName
        {
            get { return this.ClassName + "." + this.MethodName; }
        }

        public void WillThrow(string throwMessage)
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
                    msg = FullName + " 方法内部发生了异常。";
                }
                return new Exception(msg, this.Exception);
            }
        }
    }
}