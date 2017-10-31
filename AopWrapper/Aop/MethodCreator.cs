using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AopWrapper.Handlers;

namespace AopWrapper.Aop
{
    internal abstract class MethodCreator
    {
        protected readonly bool emitTryCatch;
        protected ParameterInfo[] parameters;
        protected Type[] parameterTypes;
        protected LocalBuilder localReturnValue;

        #region 静态字段

        protected const MethodAttributes METHOD_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual;
        protected const BindingFlags INTERNAL_ATTRIBUTES = BindingFlags.NonPublic | BindingFlags.Instance;

        protected static readonly Type METHOD_CONTEXT_TYPE = typeof(MethodContext);

        protected static readonly MethodInfo MethodBeginInvoke = METHOD_CONTEXT_TYPE.GetMethod("BeginInvoke", INTERNAL_ATTRIBUTES);
        protected static readonly MethodInfo MethodEndInvoke = METHOD_CONTEXT_TYPE.GetMethod("EndInvoke", INTERNAL_ATTRIBUTES);
        protected static readonly MethodInfo MethodOnException = METHOD_CONTEXT_TYPE.GetMethod("OnException", INTERNAL_ATTRIBUTES);
        protected static readonly MethodInfo MethodAddHandler = METHOD_CONTEXT_TYPE.GetMethod("AddHandler", INTERNAL_ATTRIBUTES);

        protected static readonly MethodInfo MethodGetJson = typeof(AopHandler).GetMethod("Deserialize");
        protected static readonly MethodInfo MethodGetKey = typeof(HandlerCache).GetMethod("Get");
        protected static readonly MethodInfo MethodLog = typeof(HandlerCache).GetMethod("WriteLine", new Type[] { typeof(string) });

        #endregion
        protected MethodCreator(MethodInfo methodInfo, List<string> handlers, bool emitTryCatch)
        {
            this.emitTryCatch = emitTryCatch;
            MethodInfo = methodInfo;
            Handlers = handlers;
            handlerCount = handlers?.Count ?? 0;
        }
        protected MethodCreator(MethodInfo methodInfo, List<int> handlers, bool emitTryCatch)
        {
            this.emitTryCatch = emitTryCatch;
            MethodInfo = methodInfo;
            HandlerKeys = handlers;
            handlerCount = handlers?.Count ?? 0;
        }
        public MethodInfo MethodInfo { get; }
        public List<string> Handlers { get; }
        public List<int> HandlerKeys { get; }

        private int handlerCount;


        protected ILGenerator BuildMethod(TypeBuilder typeBuilder)
        {
            parameters = MethodInfo.GetParameters();
            parameterTypes = parameters.Select(u => u.ParameterType).ToArray();

            //Type[] optionalParameterTypes = MethodInfo.GetParameters().Where(x => x.IsOptional).Select(x => x.ParameterType).ToArray();

            MethodBuilder methodBuilder = typeBuilder.DefineMethod(MethodInfo.Name, METHOD_ATTRIBUTES, MethodInfo.ReturnType, parameterTypes);

            if (MethodInfo.ContainsGenericParameters)
            {
                var gps = MethodInfo.GetGenericArguments().Select(x => x.Name).ToArray();
                if (gps.Any())
                {
                    methodBuilder.DefineGenericParameters(gps);
                }
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                methodBuilder.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
            }

            methodBuilder.SetParameters(parameterTypes);

            ILGenerator il = methodBuilder.GetILGenerator();

            DoLog(il, "-----------进入方法");

            return il;
        }

        protected LocalBuilder BuildContext(ILGenerator il)
        {
            LocalBuilder localContext = il.DeclareLocal(METHOD_CONTEXT_TYPE);

            il.Emit(OpCodes.Newobj, METHOD_CONTEXT_TYPE.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc, localContext);
            // context.MethodName = m.Name;
            il.Emit(OpCodes.Ldloc, localContext);
            il.Emit(OpCodes.Ldstr, MethodInfo.Name);
            il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("set_MethodName", INTERNAL_ATTRIBUTES), new[] { typeof(string) });
            // context.ClassName = m.DeclaringType.Name;
            il.Emit(OpCodes.Ldloc, localContext);
            il.Emit(OpCodes.Ldstr, MethodInfo.ReflectedType.Name);
            il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("set_ClassName", INTERNAL_ATTRIBUTES), new[] { typeof(string) });
            // context.Executor = this;
            il.Emit(OpCodes.Ldloc, localContext);
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("set_Executor", INTERNAL_ATTRIBUTES), new[] { typeof(object) });
            il.Emit(OpCodes.Ldloc, localContext);
            il.Emit(OpCodes.Ldtoken, MethodInfo.ReturnType);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
            il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("set_ReturnTypeOrTask", INTERNAL_ATTRIBUTES), new[] { typeof(Type) });

            #region set context.Parameters

            var parameterType = typeof(Parameter);
            LocalBuilder tmpParameters = il.DeclareLocal(typeof(Parameter[]));
            LocalBuilder par = il.DeclareLocal(parameterType);
            il.Emit(OpCodes.Ldc_I4, parameters.Length);
            il.Emit(OpCodes.Newarr, parameterType);
            il.Emit(OpCodes.Stloc, tmpParameters);

            for (int i = 0; i < parameters.Length; ++i)
            {
                il.Emit(OpCodes.Newobj, parameterType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc, par);

                il.Emit(OpCodes.Ldloc, par);
                il.Emit(OpCodes.Ldstr, parameters[i].Name);
                il.EmitCall(OpCodes.Call, parameterType.GetMethod("set_Name", INTERNAL_ATTRIBUTES), new[] { typeof(string) });

                il.Emit(OpCodes.Ldloc, par);
                var ptype = parameterTypes[i];
                if (parameterTypes[i].IsByRef)
                {
                    ptype = ptype.GetElementType();
                }
                il.Emit(OpCodes.Ldtoken, ptype);
                il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
                il.EmitCall(OpCodes.Call, parameterType.GetMethod("set_Type", INTERNAL_ATTRIBUTES), new[] { typeof(Type) });

                if (!parameters[i].IsOut)
                {
                    il.Emit(OpCodes.Ldloc, par);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    if (parameterTypes[i].IsByRef)
                    {
                        LoadIndex(il, ptype);
                    }
                    if (ptype.IsValueType || ptype.IsGenericParameter || ptype.IsGenericType)
                    {
                        il.Emit(OpCodes.Box, ptype);
                    }
                    il.EmitCall(OpCodes.Call, parameterType.GetMethod("set_Value", INTERNAL_ATTRIBUTES), new[] { typeof(object) });
                }

                il.Emit(OpCodes.Ldloc, tmpParameters);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldloc, par);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Ldloc, localContext);
            il.Emit(OpCodes.Ldloc, tmpParameters);
            il.EmitCall(OpCodes.Call, METHOD_CONTEXT_TYPE.GetMethod("set_Parameters", INTERNAL_ATTRIBUTES), new[] { typeof(object[]) });
            #endregion

            DoLog(il, "-----------记录参数完成");

            return localContext;
        }

        protected void LoadHandlers(ILGenerator il, LocalBuilder localContext)
        {
            if (Handlers == null)
            {
                foreach (var handlerKey in HandlerKeys)
                {
                    il.Emit(OpCodes.Ldloc, localContext);
                    il.Emit(OpCodes.Ldc_I4, handlerKey);
                    il.Emit(OpCodes.Call, MethodGetKey);
                    il.EmitCall(OpCodes.Call, MethodAddHandler, new[] { typeof(IAopHandler) });
                }
            }
            else
            {
                foreach (var attr in Handlers)
                {
                    il.Emit(OpCodes.Ldloc, localContext);
                    il.Emit(OpCodes.Ldstr, attr);
                    il.Emit(OpCodes.Call, MethodGetJson);
                    il.EmitCall(OpCodes.Call, MethodAddHandler, new[] { typeof(IAopHandler) });
                }
            }
            DoLog(il, "-----------记录拦截器完成");
        }

        public abstract void Create(TypeBuilder typeBuilder);

        protected void OnException(ILGenerator il, LocalBuilder localContext)
        {
            DoLog(il, "-----------开始OnException");
            il.Emit(OpCodes.Ldloc, localContext);
            il.EmitCall(OpCodes.Call, MethodOnException, Type.EmptyTypes);
        }

        protected void EndInvoke(ILGenerator il, LocalBuilder localContext)
        {
            DoLog(il, "-----------开始EndInvoke");
            il.Emit(OpCodes.Ldloc, localContext);
            il.EmitCall(OpCodes.Call, MethodEndInvoke, Type.EmptyTypes);
        }

        protected void BeginInvoke(ILGenerator il, LocalBuilder localContext)
        {
            DoLog(il, "-----------开始BeginInvoke");

            il.Emit(OpCodes.Ldloc, localContext);
            il.EmitCall(OpCodes.Call, MethodBeginInvoke, Type.EmptyTypes);

            DoLog(il, "-----------结束BeginInvoke");
        }

        protected void DoLog(ILGenerator il, string str)
        {
            //il.Emit(OpCodes.Ldstr, str);
            //il.Emit(OpCodes.Call, MethodLog);
        }

        #region 返回默认结果，Task，判断方法的返回值是不是异步模型，如果是，构建空异步模型，否则返回空

        protected static void LoadNullOrEmptyTask(ILGenerator il, Type type)
        {
            if (TaskHelper.IsGenericTask(type))//泛型异步模型
            {
                var innerType = TaskHelper.UnTask(type);
                il.Emit(OpCodes.Ldnull);
                var mthd = typeof(Task).GetMethod("FromResult").MakeGenericMethod(innerType);
                il.EmitCall(OpCodes.Call, mthd, new[] { innerType });
            }
            else if (TaskHelper.IsTask(type))//非泛型异步模型
            {
                var mthd = typeof(TaskHelper).GetMethod("Empty");
                il.EmitCall(OpCodes.Call, mthd, Type.EmptyTypes);
            }
            else//引用类型
            {
                il.Emit(OpCodes.Ldnull);
            }
        }

        #endregion

        #region LoadArgument
        /// <summary>
        /// LoadParameter
        /// </summary>
        /// <param name="il"></param>
        /// <param name="index"></param>
        protected static void LoadArgument(ILGenerator il, int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= 127)
                    {
                        il.Emit(OpCodes.Ldarg_S, index);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }
        }
        #endregion

        protected static void LoadIndex(ILGenerator il, Type type)
        {
            if (!type.IsValueType)
            {
                il.Emit(OpCodes.Ldind_Ref);
            }
            else if (type.IsEnum)
            {
                Type underType = Enum.GetUnderlyingType(type);
                LoadIndex(il, underType);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Boolean)
            {
                il.Emit(OpCodes.Ldind_I1);
            }
            else if (Type.GetTypeCode(type) == TypeCode.SByte)
            {
                il.Emit(OpCodes.Ldind_I1);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Byte)
            {
                il.Emit(OpCodes.Ldind_U1);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Int16)
            {
                il.Emit(OpCodes.Ldind_I2);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Char)
            {
                il.Emit(OpCodes.Ldind_U2);
            }
            else if (Type.GetTypeCode(type) == TypeCode.UInt16)
            {
                il.Emit(OpCodes.Ldind_U2);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Int32)
            {
                il.Emit(OpCodes.Ldind_I4);
            }
            else if (Type.GetTypeCode(type) == TypeCode.UInt32)
            {
                il.Emit(OpCodes.Ldind_U4);
            }
            else if (Type.GetTypeCode(type) == TypeCode.UInt64)
            {
                il.Emit(OpCodes.Ldind_I8);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Int64)
            {
                il.Emit(OpCodes.Ldind_I8);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Single)
            {
                il.Emit(OpCodes.Ldind_R4);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Double)
            {
                il.Emit(OpCodes.Ldind_R8);
            }
            else if (Type.GetTypeCode(type) == TypeCode.Decimal)
            {
                il.Emit(OpCodes.Ldind_R8);
            }
            else if (type == typeof(IntPtr))
            {
                il.Emit(OpCodes.Ldind_I);
            }
            else if (type == typeof(UIntPtr))
            {
                il.Emit(OpCodes.Ldind_I);
            }
            else
            {
                il.Emit(OpCodes.Ldobj, type);
            }
        }

        internal static void Create(TypeBuilder typeBuilder, MethodInfo methodInfo, List<string> handlers, bool emitTryCatch)
        {
            if (TaskHelper.IsGenericTask(methodInfo.ReturnType) || TaskHelper.IsTask(methodInfo.ReturnType))
            {
                var creator = new MethodTaskCreator(methodInfo, handlers, emitTryCatch);
                creator.Create(typeBuilder);
            }
            else
            {
                var creator = new MethodNormalCreator(methodInfo, handlers, emitTryCatch);
                creator.Create(typeBuilder);
            }
        }

        internal static void Create(TypeBuilder typeBuilder, MethodInfo methodInfo, List<int> handlers, bool emitTryCatch)
        {
            if (TaskHelper.IsGenericTask(methodInfo.ReturnType) || TaskHelper.IsTask(methodInfo.ReturnType))
            {
                var creator = new MethodTaskCreator(methodInfo, handlers, emitTryCatch);
                creator.Create(typeBuilder);
            }
            else
            {
                var creator = new MethodNormalCreator(methodInfo, handlers, emitTryCatch);
                creator.Create(typeBuilder);
            }
        }
    }
}