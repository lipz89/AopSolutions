using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using AopWrapper.Handlers;

namespace AopWrapper.Aop
{
    internal class MethodNormalCreator : MethodCreator
    {
        public MethodNormalCreator(Type targetType, MethodInfo methodInfo, FieldInfo fieldCore, List<string> handlers, bool emitTryCatch)
            : base(targetType,methodInfo,fieldCore, handlers,  emitTryCatch)
        {

        }
        public MethodNormalCreator(Type targetType, MethodInfo methodInfo, FieldInfo fieldCore, List<int> handlers, bool emitTryCatch)
            : base(targetType, methodInfo, fieldCore, handlers, emitTryCatch)
        {

        }

        public override void Create(TypeBuilder typeBuilder)
        {
            var il = this.BuildMethod(typeBuilder);

            var localContext = this.BuildContext(il);

            if (MethodInfo.ReturnType != typeof(void)) // has return value
            {
                localReturnValue = il.DeclareLocal(MethodInfo.ReturnType);
            }

            //将拦截器放到数组中，可以提取键值放缓存，此处做法是序列化后放入数据
            this.LoadHandlers(il, localContext);

            #region BeginInvoke

            this.BeginInvoke(il, localContext);

            LocalBuilder localFlag = il.DeclareLocal(typeof(bool));
            LocalBuilder localFlow = il.DeclareLocal(typeof(FlowBehavior));

            Label lblEnd = il.DefineLabel();
            //如果有返回值 Return
            if (MethodInfo.ReturnType != typeof(void) && localReturnValue != null)
            {
                Label lblReturn1 = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, localContext);
                il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("get_FlowBehavior"), new[] { typeof(FlowBehavior) });
                il.Emit(OpCodes.Stloc, localFlow);
                il.Emit(OpCodes.Ldloc, localFlow);
                il.Emit(OpCodes.Ldc_I4, (int)FlowBehavior.Return);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Stloc, localFlag);
                il.Emit(OpCodes.Ldloc, localFlag);

                //指定了新的返回值
                il.Emit(OpCodes.Brfalse, lblReturn1);

                il.Emit(OpCodes.Ldloc, localContext);
                il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("get_ReturnValueOrTask", INTERNAL_ATTRIBUTES), new[] { typeof(object) });
                if (MethodInfo.ReturnType.IsValueType || MethodInfo.ReturnType.IsGenericParameter || MethodInfo.ReturnType.IsGenericType)
                {
                    il.Emit(OpCodes.Unbox_Any, MethodInfo.ReturnType);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, MethodInfo.ReturnType);
                }
                il.Emit(OpCodes.Stloc, localReturnValue);

                il.Emit(OpCodes.Br, lblEnd);

                il.MarkLabel(lblReturn1);
            }

            #endregion

            // 如果有异常拦截器，开始try模块
            if (emitTryCatch)
            {
                il.BeginExceptionBlock(); // try {
                il.Emit(OpCodes.Ldloc, localContext);
            }

            #region  执行基类方法
            DoLog(il, "-----------开始执行基类方法");

            il.Emit(OpCodes.Ldarg_0);
            if (FieldCore != null)
            {
                il.Emit(OpCodes.Ldfld, FieldCore);
            }
            for (int i = 0; i < parameterTypes.Length; ++i)
            {
                LoadArgument(il, i + 1);
            }

            il.Emit(OpCodes.Call, MethodInfo);
            DoLog(il, "-----------执行基类方法完成");

            #endregion

            #region 如果有返回值，将返回值存到方法上下文中

            if (MethodInfo.ReturnType != typeof(void) && localReturnValue != null)
            {
                il.Emit(OpCodes.Stloc, localReturnValue);

                il.Emit(OpCodes.Ldloc, localContext);
                il.Emit(OpCodes.Ldloc, localReturnValue);
                if (MethodInfo.ReturnType.IsValueType || MethodInfo.ReturnType.IsGenericParameter || MethodInfo.ReturnType.IsGenericType)
                {
                    il.Emit(OpCodes.Box, MethodInfo.ReturnType);
                }

                il.Emit(OpCodes.Ldc_I4, emitTryCatch ? 1 : 0);
                il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("SetReturnValue", INTERNAL_ATTRIBUTES), new[] { typeof(object), typeof(bool) });
            }

            #endregion


            this.EndInvoke(il, localContext);

            //如果有异常拦截器，开始catch模块
            if (emitTryCatch)
            {
                LocalBuilder localException = il.DeclareLocal(typeof(Exception));
                il.BeginCatchBlock(typeof(Exception)); // } catch {

                #region  储存异常信息到方法上下文中

                il.Emit(OpCodes.Stloc, localException);
                il.Emit(OpCodes.Ldloc, localContext);
                il.Emit(OpCodes.Ldloc, localException);
                il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("set_Exception", INTERNAL_ATTRIBUTES), new[] { typeof(Exception) });

                #endregion

                this.OnException(il, localContext);

                #region FlowBehavior 处理异常

                Label lblRethrow = il.DefineLabel();
                Label lblThrow = il.DefineLabel();

                il.Emit(OpCodes.Ldloc, localContext);
                il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("get_FlowBehavior"), new[] { typeof(FlowBehavior) });
                il.Emit(OpCodes.Stloc, localFlow);

                //重新抛出异常 Rethrow
                il.Emit(OpCodes.Ldloc, localFlow);
                il.Emit(OpCodes.Ldc_I4, (int)FlowBehavior.RethrowException);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Stloc, localFlag);
                il.Emit(OpCodes.Ldloc, localFlag);

                il.Emit(OpCodes.Brfalse, lblRethrow);
                il.Emit(OpCodes.Rethrow);

                il.MarkLabel(lblRethrow);

                //抛出新的异常 Throw
                il.Emit(OpCodes.Ldloc, localFlow);
                il.Emit(OpCodes.Ldc_I4, (int)FlowBehavior.ThrowException);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Stloc, localFlag);
                il.Emit(OpCodes.Ldloc, localFlag);

                il.Emit(OpCodes.Brfalse, lblThrow);
                il.Emit(OpCodes.Ldloc, localContext);
                il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("NewException", INTERNAL_ATTRIBUTES), new[] { typeof(Exception) });
                il.Emit(OpCodes.Throw);

                il.MarkLabel(lblThrow);

                //如果有返回值 Return
                if (MethodInfo.ReturnType != typeof(void) && localReturnValue != null)
                {
                    Label lblReturn = il.DefineLabel();
                    Label lblDefault = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, localFlow);
                    il.Emit(OpCodes.Ldc_I4, (int)FlowBehavior.Return);
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Stloc, localFlag);
                    il.Emit(OpCodes.Ldloc, localFlag);

                    //指定了新的返回值
                    il.Emit(OpCodes.Brfalse, lblReturn);
                    il.Emit(OpCodes.Ldloc, localContext);
                    il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("get_ReturnValueOrTask", INTERNAL_ATTRIBUTES), new[] { typeof(object) });

                    if (MethodInfo.ReturnType.IsValueType || MethodInfo.ReturnType.IsGenericParameter || MethodInfo.ReturnType.IsGenericType)
                    {
                        il.Emit(OpCodes.Unbox_Any, MethodInfo.ReturnType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Castclass, MethodInfo.ReturnType);
                    }
                    il.Emit(OpCodes.Stloc, localReturnValue);
                    il.Emit(OpCodes.Br, lblDefault);

                    il.MarkLabel(lblReturn);

                    //返回默认值
                    if (MethodInfo.ReturnType.IsValueType)
                    {
                        var local = il.DeclareLocal(MethodInfo.ReturnType);
                        il.Emit(OpCodes.Ldloca, local);
                        il.Emit(OpCodes.Initobj, MethodInfo.ReturnType);
                        il.Emit(OpCodes.Ldloc, local);
                        il.Emit(OpCodes.Stloc, localReturnValue);
                    }
                    else
                    {
                        LoadNullOrEmptyTask(il, MethodInfo.ReturnType);
                        //il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stloc, localReturnValue);
                    }
                    il.MarkLabel(lblDefault);
                }

                #endregion

                il.EndExceptionBlock(); // }
            }

            // 如果方法有返回值，将返回值返回
            il.MarkLabel(lblEnd);

            if (MethodInfo.ReturnType != typeof(void) && localReturnValue != null)
            {
                il.Emit(OpCodes.Ldloc, localReturnValue);
            }

            il.Emit(OpCodes.Ret);
        }
    }
}
