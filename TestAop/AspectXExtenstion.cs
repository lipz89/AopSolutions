using System;
using System.Diagnostics;
using AopWrapper;
using AspectX;

namespace TestAop
{
    static class AspectXExtenstion
    {
        /// <summary>
        /// 织入日志操作，在方法执行前后分别写入日志
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Worker WithLog(this Worker worker, string message)
        {
            return worker.With((work) =>
            {
                var mthdName = worker.Context.FullName;
                Logger.Info(mthdName + " " + "开始");
                if (message.IsNotNullOrEmptyOrWhiteSpace())
                {
                    Logger.Info(message);
                }
                work();
                Logger.Info(mthdName + " " + "结束");
            });
        }
        public static Worker WithMonitor(this Worker worker)
        {
            return worker.With((work) =>
            {
                var st = Stopwatch.StartNew();
                work();
                st.Stop();
                var time = st.ElapsedMilliseconds;
                Logger.Info(worker.Context.FullName + "方法耗时结束" + time + "毫秒。");
            });
        }

        /// <summary>
        /// 织入异常处理
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="exceptionAction"></param>
        /// <returns></returns>
        public static Worker WithTryCatch(this Worker worker, Action<WorkContext> exceptionAction)
        {
            return worker.With((work) =>
            {
                try
                {
                    work();
                }
                catch (Exception ex)
                {
                    worker.Context.SetException(ex);
                    var mthdName = worker.Context.FullName;
                    Logger.Info(mthdName + " 发生异常");
                    exceptionAction(worker.Context);
                    switch (worker.Context.FlowBehavior)
                    {
                        case FlowBehavior.RethrowException:
                            throw;
                        case FlowBehavior.ThrowException:
                            throw worker.Context.ThrowException;
                    }
                }
            });
        }

        /// <summary>
        /// 织入日志操作，在方法执行前后分别写入日志
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Worker<T> WithLog<T>(this Worker<T> worker, string message)
        {
            return worker.With((work) =>
            {
                var mthdName = worker.Context.FullName;
                Logger.Info(mthdName + " " + "开始");
                if (message.IsNotNullOrEmptyOrWhiteSpace())
                {
                    Logger.Info(message);
                }

                var rst = work();

                Logger.Info(mthdName + " " + "结束");

                return rst;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="exceptionAction"></param>
        /// <returns></returns>
        public static Worker<T> WithTryCatch<T>(this Worker<T> worker, Action<WorkContext<T>> exceptionAction)
        {
            return worker.With((work) =>
            {
                try
                {
                    return work();
                }
                catch (Exception ex)
                {
                    worker.Context.SetException(ex);
                    var mthdName = worker.Context.FullName;
                    Logger.Info(mthdName + " 发生异常");
                    exceptionAction(worker.Context);
                    switch (worker.Context.FlowBehavior)
                    {
                        case FlowBehavior.Return:
                            if (worker.Context.ReturnValue != null)
                            {
                                return worker.Context.ReturnValue;
                            }
                            throw;
                        case FlowBehavior.ThrowException:
                            throw worker.Context.ThrowException;
                        default:
                            throw;
                    }
                }
            });
        }
    }
}