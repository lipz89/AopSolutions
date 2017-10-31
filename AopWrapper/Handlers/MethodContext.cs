using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace AopWrapper.Handlers
{
    /// <summary>
    /// ����ִ��������
    /// </summary>
    public class MethodContext
    {
        /// <summary> ����ִ�еĵ����� </summary>
        public object Executor { get; internal set; }
        /// <summary> ������ </summary>
        public string ClassName { get; internal set; }
        /// <summary> �������� </summary>
        public string MethodName { get; internal set; }
        /// <summary> ����ֵ </summary>
        internal object ReturnValueOrTask { get; private set; }
        /// <summary> ����ֵ��Task </summary>
        public object ReturnValue { get; private set; }
        /// <summary> ����ֵ���� </summary>
        public Type ReturnTypeOrTask { get; internal set; }
        /// <summary> ����ֵ����,��Task </summary>
        public Type ReturnType
        {
            get { return TaskHelper.UnTask(this.ReturnTypeOrTask); }
        }
        /// <summary> �������� </summary>
        public FlowBehavior FlowBehavior { get; internal set; }
        /// <summary> �����б� </summary>
        public Parameter[] Parameters { get; internal set; }
        /// <summary> �쳣��Ϣ </summary>
        public Exception Exception { get; internal set; }

        #region Handlers
        /// <summary> ������������ </summary>
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

        /// <summary> �쳣��Ϣ </summary>
        internal string ThrowMessage { get; set; }

        /// <summary> ����һ����װ������쳣 </summary>
        internal Exception NewException()
        {
            var msg = this.ThrowMessage;
            if (string.IsNullOrWhiteSpace(msg))
            {
                msg = FullName + " �����ڲ��������쳣��";
            }
            return new Exception(msg, this.Exception);
        }

        /// <summary> ���������������ͷ����� </summary>
        public string FullName
        {
            get { return this.ClassName + "." + this.MethodName; }
        }

        /// <summary>
        /// �÷�������һ��ָ��ֵ����ָ��ֵ���ظ����͵�Ĭ��ֵ
        /// </summary>
        /// <param name="returnValue">ָ��ֵ</param>
        public void WillReturn(object returnValue = null)
        {
            this.FlowBehavior = FlowBehavior.Return;
            TrySetReturnValue(returnValue);
        }

        /// <summary>
        /// �÷����׳�һ���µ��쳣�����쳣����װԭ�е��쳣��Ϣ
        /// </summary>
        /// <param name="throwMessage">�쳣��Ϣ</param>
        public void WillThrow(string throwMessage = null)
        {
            if (Exception != null)
            {
                this.ThrowMessage = throwMessage;
                this.FlowBehavior = FlowBehavior.ThrowException;
            }
        }
        /// <summary>
        /// �����׳�ԭ���쳣
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
                throw new InvalidCastException(string.Format("����{0}����ת��Ϊ����{1}", returnValue.GetType().FullName, ReturnType.FullName));
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