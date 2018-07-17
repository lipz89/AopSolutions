using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using AopDecorator.Handlers;

namespace AopDecorator
{
    public static class Interceptor
    {
        public static object Invoke(object @object, string @method, object[] parameters, Type[] types)
        {
            //要执行的方法和方法所属实体
            var tgtType = @object.GetType();
            var mthd = tgtType.GetMethod(@method, types);

            #region 查找方法定义的拦截器
            bool hasExcpetions;
            var attrs = GetHandlers(mthd, tgtType, out hasExcpetions);

            #endregion

            #region 没有拦截器直接执行方法

            if (attrs.Count == 0)
            {
                return mthd.Invoke(@object, parameters);
            }
            #endregion

            #region 方法执行上下文，包含参数，返回值类型，等
            var pars = new List<Parameter>();

            var ps = mthd.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                pars.Add(new Parameter
                {
                    Name = ps[i].Name,
                    Type = ps[i].ParameterType,
                    Value = parameters[i]
                });
            }

            var ctx = new MethodContext
            {
                Executor = @object,
                MethodName = @method,
                Parameters = pars.ToArray(),
                ReturnType = mthd.ReturnType
            };

            #endregion

            //应用拦截器
            Func<object> dotry = () =>
            {
                attrs.ForEach(x => x.BeginInvoke(ctx));

                if (ctx.FlowBehavior == FlowBehavior.Return)
                {
                    if (TaskHelper.IsTask(ctx.ReturnType))
                    {
                        return TaskHelper.Empty();
                    }

                    if (TaskHelper.IsGenericTask(ctx.ReturnType))
                    {
                        return TaskHelper.FromResult(ctx.RealReturnType, ctx.ReturnValue);
                    }

                    if (ctx.ReturnValue == null || ctx.ReturnType.IsInstanceOfType(ctx.ReturnValue))
                    {
                        return ctx.ReturnValue;
                    }
                }

                var retObj = mthd.Invoke(@object, parameters);
                if (TaskHelper.IsGenericTask(ctx.ReturnType))
                {
                    retObj = TaskHelper.With(retObj, ctx.RealReturnType, (t) =>
                    {
                        ctx.ReturnValue = t;
                        attrs.ForEachReverse(x => x.EndInvoke(ctx));
                    });
                }
                else if (ctx.RealReturnType != null && ctx.RealReturnType != typeof(void))
                {
                    ctx.ReturnValue = retObj;
                    attrs.ForEachReverse(x => x.EndInvoke(ctx));
                }
                else
                {
                    attrs.ForEachReverse(x => x.EndInvoke(ctx));
                }

                return retObj;
            };

            //没有异常拦截器就直接执行
            if (!hasExcpetions) { return dotry(); }

            //异常处理部分
            Action<Exception> docatch = (ex) =>
            {
                ctx.Exception = ex;

                attrs.ForEach(x => x.OnException(ctx));
            };

            #region 有异常拦截器就开启一个try/catch
            try
            {
                return dotry();
            }
            catch (Exception ex)
            {
                docatch(ex);
                switch (ctx.FlowBehavior)
                {
                    case FlowBehavior.Return:
                        if (TaskHelper.IsTask(ctx.ReturnType))
                        {
                            return TaskHelper.Empty();
                        }
                        if (TaskHelper.IsGenericTask(ctx.ReturnType))
                        {
                            return TaskHelper.FromResult(ctx.RealReturnType, ctx.ReturnValue);
                        }
                        if (ctx.ReturnType != null && ctx.ReturnType != typeof(void))
                        {
                            return ctx.ReturnValue;
                        }
                        return DefaultValue(mthd.ReturnType);
                    case FlowBehavior.ThrowException:
                        throw ctx.ThrowException ?? new Exception(ctx.ThrowMessage, ex);
                    default:
                        throw;
                }
            }
            #endregion
        }

        private static object TryOrNot(Func<object> dotry, Func<Exception, MethodContext> docatch, object dflValue)
        {
            try
            {
                return dotry();
            }
            catch (Exception ex)
            {
                var ctx = docatch(ex);
                switch (ctx.FlowBehavior)
                {
                    case FlowBehavior.Return:
                        if (TaskHelper.IsTask(ctx.ReturnType))
                        {
                            return TaskHelper.Empty();
                        }
                        if (TaskHelper.IsGenericTask(ctx.ReturnType))
                        {
                            return TaskHelper.FromResult(ctx.RealReturnType, ctx.ReturnValue);
                        }
                        if (ctx.ReturnType != null && ctx.ReturnType != typeof(void))
                        {
                            return ctx.ReturnValue;
                        }
                        return dflValue;
                    case FlowBehavior.ThrowException:
                        throw new Exception("执行错误。", ex);
                    default:
                        throw;
                }
            }
        }

        private static object DefaultValue(Type t)
        {
            if (t == typeof(void) || !t.IsValueType)
            {
                return null;
            }
            var dft = Expression.Default(t);
            var lbd = Expression.Lambda(dft);
            return lbd.Compile().DynamicInvoke();
        }

        private static List<AopHandler> GetHandlers(MethodInfo method, Type innerType, out bool hasExcpetions)
        {
            var types = method.GetParameters().Select(x => x.ParameterType).ToArray();
            var mi = innerType.GetMethod(method.Name, types);

            var baseHandlers = innerType.GetCustomAttributes<AopHandler>().ToList();
            var hanlders = mi?.GetCustomAttributes<AopHandler>().ToList() ?? new List<AopHandler>();

            foreach (var baseHandler in baseHandlers)
            {
                if (hanlders.All(x => x.GetType() != baseHandler.GetType()))
                {
                    hanlders.Add(baseHandler);
                }
            }

            hasExcpetions = false;

            foreach (var attr in hanlders)
            {
                if (attr.GetType().GetMethod("OnException").DeclaringType != typeof(AopHandler))
                {
                    hasExcpetions = true;
                }
            }

            return hanlders.OrderBy(x => x.Order).ToList();
        }
    }
}