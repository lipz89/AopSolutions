using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using AopWrapper.Handlers;

namespace AopWrapper.Aop
{
    internal class MethodTaskCreator : MethodCreator
    {
        public MethodTaskCreator(MethodInfo methodInfo, List<string> handlers, bool emitTryCatch)
            : base(methodInfo, handlers, emitTryCatch)
        {

        }
        public MethodTaskCreator(MethodInfo methodInfo, List<int> handlers, bool emitTryCatch)
            : base(methodInfo, handlers, emitTryCatch)
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

            DoLog(il, "-----------bool");
            LocalBuilder localFlag = il.DeclareLocal(typeof(bool));
            DoLog(il, "-----------FlowBehavior");
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

            #region  执行基类方法

            DoLog(il, "-----------开始执行基类方法");

            il.Emit(OpCodes.Ldarg_0);
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


                il.Emit(OpCodes.Ldloc, localContext);
                il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("get_ReturnValueOrTask", INTERNAL_ATTRIBUTES), new[] { typeof(object) });
                if (MethodInfo.ReturnType.IsValueType || MethodInfo.ReturnType.IsGenericParameter || MethodInfo.ReturnType.IsGenericType)
                {
                    il.Emit(OpCodes.Unbox_Any, MethodInfo.ReturnType);
                }
                il.Emit(OpCodes.Stloc, localReturnValue);

                il.MarkLabel(lblEnd);

                il.Emit(OpCodes.Ldloc, localReturnValue);
            }
            #endregion
            DoLog(il, "-----------方法执行结束");

            il.Emit(OpCodes.Ret);
        }
    }
}