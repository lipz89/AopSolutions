using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectX
{
    public static class Aspect
    {
        /// <summary>
        /// ��ʼһ������
        /// </summary>
        /// <returns></returns>
        public static Worker Work(Action work)
        {
            Check.NotNull(work, nameof(work));

            var ctx = CreateWorkContext();
            return new Worker(ctx, work);
        }

        /// <summary>
        /// ��ʼһ���з���ֵ�Ĺ���
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Worker<T> Work<T>(Func<T> work)
        {
            Check.NotNull(work, nameof(work));

            var ctx = CreateWorkContext<T>();
            return new Worker<T>(ctx, work);
        }

        /// <summary>
        /// ��ʼҪһ���޷���ֵ��������
        /// </summary>
        /// <returns></returns>
        public static WorkerTask Task(Func<Task> work)
        {
            Check.NotNull(work, nameof(work));

            var ctx = CreateWorkContext();
            return new WorkerTask(ctx, work);
        }

        /// <summary>
        /// ��ʼҪһ���з���ֵ��������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static WorkerTaskT<T> Task<T>(Func<Task<T>> work)
        {
            Check.NotNull(work, nameof(work));

            var ctx = CreateWorkContext<T>();
            return new WorkerTaskT<T>(ctx, work);
        }

        private static WorkContext CreateWorkContext()
        {
            var ctx = new WorkContext();
            var trace = new StackTrace();
            var frames = trace.GetFrames();
            if (frames != null)
            {
                var index = 2;
                var frame = frames[index];
                var mthd = frame.GetMethod();
                while (SkipType(mthd.ReflectedType))
                {
                    index++;
                    frame = frames[index];
                    mthd = frame.GetMethod();
                }
                if (mthd.ReflectedType != null)
                {
                    ctx.ClassName = mthd.ReflectedType.Name;
                }
                ctx.MethodName = mthd.Name;
                var mi = mthd as MethodInfo;
                if (mi != null)
                {
                    ctx.ReturnType = typeof(void);
                }
            }
            return ctx;
        }
        private static WorkContext<T> CreateWorkContext<T>()
        {
            var ctx = new WorkContext<T>();
            var trace = new StackTrace();
            var frames = trace.GetFrames();
            if (frames != null)
            {
                var index = 2;
                var frame = frames[index];
                var mthd = frame.GetMethod();
                while (SkipType(mthd.ReflectedType))
                {
                    index++;
                    frame = frames[index];
                    mthd = frame.GetMethod();
                }
                if (mthd.ReflectedType != null)
                {
                    ctx.ClassName = mthd.ReflectedType.Name;
                }
                ctx.MethodName = mthd.Name;
                var mi = mthd as MethodInfo;
                if (mi != null)
                {
                    ctx.ReturnType = mi.ReturnType;
                }
            }
            return ctx;
        }

        private static bool SkipType(Type type)
        {
            if (type.IsAnonymousType())
            {
                return true;
            }

            if (type.IsGenericType)
            {
                var gt = type.GetGenericTypeDefinition();
                return gt == typeof(AsyncTaskMethodBuilder<>);
            }
            return type == typeof(AsyncTaskMethodBuilder);
        }
    }
}