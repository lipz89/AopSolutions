using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace AopWrapper.Handlers
{
    /// <summary>
    /// 方法执行上下文
    /// </summary>
    public class MethodContext
    {
        /// <summary> 方法执行的调用者 </summary>
        public object Executor { get; internal set; }
        /// <summary> 类名称 </summary>
        public string ClassName { get; internal set; }
        /// <summary> 方法名称 </summary>
        public string MethodName { get; internal set; }
        /// <summary> 返回值 </summary>
        internal object ReturnValueOrTask { get; private set; }
        /// <summary> 返回值非Task </summary>
        public object ReturnValue { get; private set; }
        /// <summary> 返回值类型 </summary>
        public Type ReturnTypeOrTask { get; internal set; }
        /// <summary> 返回值类型,非Task </summary>
        public Type ReturnType
        {
            get { return TaskHelper.UnTask(this.ReturnTypeOrTask); }
        }
        /// <summary> 后续动作 </summary>
        public FlowBehavior FlowBehavior { get; internal set; }
        /// <summary> 参数列表 </summary>
        public Parameter[] Parameters { get; internal set; }
        /// <summary> 异常信息 </summary>
        public Exception Exception { get; internal set; }

        #region Handlers
        /// <summary> 方法的拦截器 </summary>
        private IList<IAopHandler> handlers = new List<IAopHandler>();

        internal void AddHandler(IAopHandler handler)
        {
            this.handlers.Add(handler);
        }

        internal void BeginInvoke()
        {
            foreach (var aopHandler in this.handlers)
            {
                aopHandler?.BeginInvoke(this);
            }
        }

        internal void EndInvoke()
        {
            foreach (var aopHandler in this.handlers)
            {
                aopHandler?.EndInvoke(this);
            }
        }

        internal void OnException()
        {
            foreach (var aopHandler in this.handlers)
            {
                aopHandler?.OnException(this);
            }
        }

        #endregion

        /// <summary> 异常信息 </summary>
        internal string ThrowMessage { get; set; }

        /// <summary> 返回一个包装后的新异常 </summary>
        internal Exception NewException()
        {
            var msg = this.ThrowMessage;
            if (string.IsNullOrWhiteSpace(msg))
            {
                msg = FullName + " 方法内部发生了异常。";
            }
            return new Exception(msg, this.Exception);
        }

        /// <summary> 方法所属的类名和方法名 </summary>
        public string FullName
        {
            get { return this.ClassName + "." + this.MethodName; }
        }

        /// <summary>
        /// 让方法返回一个指定值，不指定值返回该类型的默认值
        /// </summary>
        /// <param name="returnValue">指定值</param>
        public void WillReturn(object returnValue = null)
        {
            this.FlowBehavior = FlowBehavior.Return;
            TrySetReturnValue(returnValue);
        }

        /// <summary>
        /// 让方法抛出一个新的异常，该异常将包装原有的异常信息
        /// </summary>
        /// <param name="throwMessage">异常信息</param>
        public void WillThrow(string throwMessage = null)
        {
            if (Exception != null)
            {
                this.ThrowMessage = throwMessage;
                this.FlowBehavior = FlowBehavior.ThrowException;
            }
        }
        /// <summary>
        /// 重新抛出原有异常
        /// </summary>
        public void WillRethrow()
        {
            if (Exception != null)
            {
                this.FlowBehavior = FlowBehavior.RethrowException;
            }
        }
        internal void SetReturnValue(object returnValue, bool withTryCatch)
        {
            if (ReturnTypeOrTask == null || ReturnTypeOrTask == typeof(void))
            {
                return;
            }

            var doTryCatch = this.handlers.Any() && withTryCatch;

            if (TaskHelper.IsGenericTask(ReturnTypeOrTask))
            {
                var wrapper = GetGWrapper(returnValue, ReturnType, doTryCatch);
                ReturnValueOrTask = wrapper;
            }
            else if (TaskHelper.IsTask(ReturnTypeOrTask))
            {
                var wrapper = GetWrapper(returnValue, withTryCatch);
                ReturnValueOrTask = wrapper;
            }
            else
            {
                ReturnValueOrTask = returnValue;
                ReturnValue = returnValue;
            }
        }

        internal void TrySetReturnValue(object returnValue)
        {
            if (ReturnTypeOrTask == null || ReturnTypeOrTask == typeof(void))
            {
                return;
            }

            if (TaskHelper.IsTask(ReturnTypeOrTask))
            {
                ReturnValueOrTask = TaskHelper.Empty();
                return;
            }

            object obj = null;
            if (returnValue == null)
            {
                if (ReturnType.IsValueType)
                {
                    obj = GetDefaultValue(ReturnType);
                }
            }
            else if (ReturnType.IsInstanceOfType(returnValue))
            {
                obj = returnValue;
            }
            else if (ReturnTypeOrTask.IsInstanceOfType(returnValue))
            {
                ReturnValueOrTask = returnValue;
                return;
            }
            else
            {
                throw new InvalidCastException(string.Format("类型{0}不能转换为类型{1}", returnValue.GetType().FullName, ReturnType.FullName));
            }


            if (ReturnType == ReturnTypeOrTask)
            {
                ReturnValueOrTask = obj;
                ReturnValue = obj;
            }
            else if (TaskHelper.IsGenericTask(ReturnTypeOrTask))
            {
                ReturnValueOrTask = TaskHelper.FromResult(ReturnType, obj);
            }
        }

        private object GetDefaultValue(Type type)
        {
            var e = Expression.Default(type);
            var lbd = Expression.Lambda(e);
            return lbd.Compile().DynamicInvoke();
        }

        private object GetWrapper(object task, bool withTryCatch)
        {
            var thetask = (Task)task;

            var rtntask = thetask.ContinueWith(t =>
            {
                this.EndInvoke();
            });

            if (withTryCatch)
            {
                rtntask = rtntask.ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        this.Exception = t.Exception;
                        this.OnException();

                        if (this.FlowBehavior == FlowBehavior.RethrowException)
                        {
                            throw t.Exception;
                        }

                        if (this.FlowBehavior == FlowBehavior.ThrowException)
                        {
                            throw this.NewException();
                        }
                    }
                });
            }
            return rtntask;
        }

        private object GetGWrapper(object task, Type type, bool withTryCatch)
        {
            var mthd = this.GetType().GetMethod("GWrapper", BindingFlags.NonPublic | BindingFlags.Instance);
            mthd = mthd.MakeGenericMethod(type);
            var val = mthd.Invoke(this, new object[] { task, withTryCatch });
            return val;
        }

        private object GWrapper<T>(Task<T> task, bool withTryCatch)
        {
            var rtntask = task.ContinueWith<T>(t =>
            {
                this.ReturnValue = t.Result;
                this.EndInvoke();

                return t.Result;
            });
            if (withTryCatch)
            {
                rtntask = rtntask.ContinueWith<T>(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        this.Exception = t.Exception;
                        this.OnException();

                        if (this.FlowBehavior == FlowBehavior.RethrowException)
                        {
                            throw t.Exception;
                        }

                        if (this.FlowBehavior == FlowBehavior.ThrowException)
                        {
                            throw this.NewException();
                        }

                        if (this.FlowBehavior == FlowBehavior.Return)
                        {
                            return ((Task<T>)this.ReturnValueOrTask).Result;
                        }

                        return default(T);
                    }
                    return t.Result;
                });
            }
            return rtntask;
        }
    }
}