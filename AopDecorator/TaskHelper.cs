using System;
using System.Linq;
using System.Threading.Tasks;

namespace AopDecorator
{
    public static class TaskHelper
    {
        public static Type UnTask(Type type)
        {
            if (IsTask(type))
            {
                return typeof(void);
            }

            if (IsGenericTask(type))
            {
                return type.GetGenericArguments().FirstOrDefault();
            }
            return type;
        }

        public static bool IsTask(Type type)
        {
            if (type == null)
            {
                return false;
            }
            if (type == typeof(Task))
            {
                return true;
            }
            return false;
        }
        public static bool IsGenericTask(Type type)
        {
            if (type == null)
            {
                return false;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return true;
            }
            return false;
        }

        public static Task Empty()
        {
            return Task.Run(() => { });
        }

        public static object FromResult(Type type, object result)
        {
            if (result != null && !type.IsInstanceOfType(result))
            {
                throw new InvalidCastException(string.Format("类型{0}无法转换成类型{1}。", result.GetType(), type));
            }
            var mthd = typeof(Task).GetMethod("FromResult").MakeGenericMethod(type);
            return mthd.Invoke(null, new[] { result });
        }

        public static Task<T> With<T>(Task<T> task, Action<object> work)
        {
            return task.ContinueWith((t) =>
                                     {
                                         switch (t.Status)
                                         {
                                             case TaskStatus.RanToCompletion:
                                                 work(t.Result);
                                                 return t.Result;
                                             case TaskStatus.Faulted:
                                                 throw new Exception("异步方法执行错误", t.Exception);
                                             default:
                                                 return default(T);
                                         }
                                     });
        }

        public static object With(object task, Type type, Action<object> work)
        {
            var taskType = task.GetType();
            if (!IsGenericTask(taskType))
            {
                throw new InvalidCastException(string.Format("类型{0}无法转换成类型{1}。", taskType, typeof(Task<>)));
            }
            taskType = TaskHelper.UnTask(task.GetType());
            if (taskType != type)
            {
                throw new InvalidCastException(string.Format("类型{0}无法转换成类型{1}。", type, taskType));
            }
            var mthd = typeof(TaskHelper).GetMethods().FirstOrDefault(x => x.Name == "With" && x.IsGenericMethod);
            mthd = mthd.MakeGenericMethod(type);
            return mthd.Invoke(null, new object[] { task, work });
        }
    }
}
