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
            bool hasExcpetions;
            var tgtType = @object.GetType();
            var mthd = tgtType.GetMethod(@method, types);

            var attrs = GetHandlers(mthd, tgtType, out hasExcpetions);

            if (attrs.Count == 0)
            {
                return mthd.Invoke(@object, parameters);
            }

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
                ClassName = @object.GetType().Name,
                Executor = @object,
                MethodName = @method,
                Parameters = pars.ToArray(),
                ReturnType = mthd.ReturnType
            };

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

            if (!hasExcpetions)
            {
                return dotry();
            }

            Action<Exception> docatch = (ex) =>
            {
                ctx.Exception = ex;

                attrs.ForEach(x => x.OnException(ctx));
            };


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
                        throw new Exception("Ö´ÐÐ´íÎó¡£", ex);
                    default:
                        throw;
                }
            }

            //var dflVal = DefaultValue(mthd.ReturnType);

            //return TryOrNot(dotry, docatch, dflVal);
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
                        throw new Exception("Ö´ÐÐ´íÎó¡£", ex);
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

        private static List<BaseHandler> GetHandlers(MethodInfo method, Type innerType, out bool hasExcpetions)
        {
            var types = method.GetParameters().Select(x => x.ParameterType).ToArray();
            var mi = innerType.GetMethod(method.Name, types);

            var baseHandlers = innerType.GetCustomAttributes<BaseHandler>().ToList();
            var hanlders = mi?.GetCustomAttributes<BaseHandler>().ToList() ?? new List<BaseHandler>();

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
                if (attr.GetType().GetMethod("OnException").DeclaringType != typeof(BaseHandler))
                {
                    hasExcpetions = true;
                }
            }

            return hanlders.OrderBy(x => x.Order).ToList();
        }
    }
}