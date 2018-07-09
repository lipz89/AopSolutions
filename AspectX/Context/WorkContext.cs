using System;

namespace AspectX
{
    /// <summary>
    /// 工作上下文
    /// </summary>
    public class WorkContext
    {
        /// <summary> 类名称 </summary>
        public string ClassName { get; internal set; }

        /// <summary> 方法名称 </summary>
        public string MethodName { get; internal set; }

        /// <summary> 返回值类型 </summary>
        public Type ReturnType { get; internal set; }

        /// <summary> 返回值类型非Task </summary>
        public Type RealReturnType
        {
            get { return TaskHelper.UnTask(this.ReturnType); }
        }

        /// <summary> 后续动作 </summary>
        public FlowBehavior FlowBehavior { get; protected set; }

        /// <summary> 异常信息 </summary>
        public Exception Exception { get; protected set; }

        /// <summary> 抛出异常时的异常信息 </summary>
        public string ThrowMessage { get; protected set; }

        /// <summary> 类名加方法名 </summary>
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
                return new Exception(this.ThrowMessage ?? FullName + " 方法内部发生了异常。", this.Exception);
            }
        }
    }  /// <summary>
       /// 工作上下文
       /// </summary>
    public class WorkContext<T> : WorkContext
    {
        /// <summary> 返回值 </summary>
        public T ReturnValue { get; set; }

        public void WillReturn(T returnValue = default(T))
        {
            this.FlowBehavior = FlowBehavior.Return;
            this.ReturnValue = returnValue;
        }
    }
}