using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AopDecorator
{
    public static class Proxy
    {
        private const string ASSEMBLY_NAME = "ProxyAssembly";
        private const string MODULE_NAME = "ProxyModule";
        private const string TYPE_NAME = "Proxy_";

        private static readonly AssemblyBuilder assembly;
        private static readonly ModuleBuilder module;
        private static readonly Hashtable typeCache = Hashtable.Synchronized(new Hashtable());

        static Proxy()
        {
            AssemblyName assemblyName = new AssemblyName { Name = ASSEMBLY_NAME };
            assemblyName.SetPublicKey(Assembly.GetExecutingAssembly().GetName().GetPublicKey());

            assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            module = assembly.DefineDynamicModule(MODULE_NAME, ASSEMBLY_NAME + ".dll");
        }

        //[Conditional("XDEBUG")]
        public static void Save()
        {
            assembly.Save(ASSEMBLY_NAME + ".dll");
        }

        public static object Of(Type baseType, params object[] parameters)
        {
            Type proxyType = CreateType(baseType);

            return FastObjectCreator.CreateObject(proxyType, parameters);
        }
        public static T Of<T>(params object[] parameters) where T : class
        {
            Type proxyType = CreateType<T>();

            return (T)FastObjectCreator.CreateObject(proxyType, parameters);
        }
        public static object Of(Type innerType, Type baseType, params object[] parameters)
        {
            Type proxyType = CreateType(innerType, baseType);

            return FastObjectCreator.CreateObject(proxyType, parameters);
        }
        public static TBase Of<T, TBase>(params object[] parameters) where T : class, TBase
        {
            Type proxyType = CreateType<T, TBase>();

            return (TBase)FastObjectCreator.CreateObject(proxyType, parameters);
        }
        public static Type CreateType(Type innerType, Type baseType = null)
        {
            if (baseType == null)
            {
                baseType = innerType;
            }
            else if (!baseType.IsAssignableFrom(innerType))
            {
                throw new Exception(string.Format("类型{0}不能从类型{1}分配实例。", baseType, innerType));
            }
            lock (typeCache.SyncRoot)
            {
                Type proxyType = typeCache[baseType] as Type;

                if (proxyType == null)
                {
                    proxyType = CreateTypeCore(innerType, baseType);
                    typeCache.Add(baseType, proxyType);
                }
                return proxyType;
            }
        }
        public static Type CreateType<T, TBase>() where T : class, TBase
        {
            return CreateType(typeof(T), typeof(TBase));
        }
        public static Type CreateType<T>() where T : class
        {
            return CreateType(typeof(T));
        }
        private static Type CreateTypeCore(Type innerType, Type baseType)
        {
            string nameOfType = TYPE_NAME + innerType.Name;

            TypeBuilder typeBuilder;
            if (baseType.IsInterface)
            {
                typeBuilder = module.DefineType(nameOfType, TypeAttributes.Public);
                typeBuilder.AddInterfaceImplementation(baseType);
            }
            else
            {
                typeBuilder = module.DefineType(nameOfType, TypeAttributes.Public, baseType);
            }

            InjectInterceptor(innerType, baseType, typeBuilder);

            var t = typeBuilder.CreateType();


            return t;
        }

        private static void InjectInterceptor(Type innerType, Type baseType, TypeBuilder typeBuilder)
        {
            // ---- define costructors ----
            var fieldCore = BuildConstructor(innerType, baseType, typeBuilder);

            // ---- define methods ----
            var methodsOfType = GetMethods(baseType);

            foreach (var method in methodsOfType)
            {
                if (method == null)
                    continue;

                var methodParameterTypes = method.GetParameters();
                Type[] parameterTypes = methodParameterTypes.Select(u => u.ParameterType).ToArray();

                var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot, method.ReturnType, parameterTypes);
                if (method.ContainsGenericParameters)
                {
                    var gps = method.GetGenericArguments().Select(x => x.Name).ToArray();
                    if (gps.Any())
                        methodBuilder.DefineGenericParameters(gps);
                }
                for (int j = 0; j < methodParameterTypes.Length; j++)
                {
                    methodBuilder.DefineParameter(j + 1, methodParameterTypes[j].Attributes, methodParameterTypes[j].Name);
                }

                methodBuilder.SetParameters(parameterTypes);

                var ilOfMethod = methodBuilder.GetILGenerator();
                //ilOfMethod.Emit(OpCodes.Ldarg_0);
                //ilOfMethod.Emit(OpCodes.Ldfld, fieldInterceptor);
                ilOfMethod.Emit(OpCodes.Ldarg_0);
                ilOfMethod.Emit(OpCodes.Ldfld, fieldCore);
                
                //ilOfMethod.Emit(OpCodes.Newobj, typeof(T).GetConstructor(new Type[0]));
                ilOfMethod.Emit(OpCodes.Ldstr, method.Name);
                
                var parameters = ilOfMethod.DeclareLocal(typeof(object[]));
                ilOfMethod.Emit(OpCodes.Ldc_I4, methodParameterTypes.Length);
                ilOfMethod.Emit(OpCodes.Newarr, typeof(object));
                ilOfMethod.Emit(OpCodes.Stloc, parameters);

                var pts = ilOfMethod.DeclareLocal(typeof(Type[]));
                ilOfMethod.Emit(OpCodes.Ldc_I4, methodParameterTypes.Length);
                ilOfMethod.Emit(OpCodes.Newarr, typeof(Type));
                ilOfMethod.Emit(OpCodes.Stloc, pts);

                for (var i = 0; i < methodParameterTypes.Length; i++)
                {
                    ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                    ilOfMethod.Emit(OpCodes.Ldc_I4, i);
                    if (parameterTypes[i].IsByRef)
                    {
                        if (methodParameterTypes[i].IsOut)
                        {
                            LoadDefaultValue(ilOfMethod, parameterTypes[i].GetElementType());
                        }
                        else
                        {
                            ilOfMethod.Emit(OpCodes.Ldarg, i + 1);
                            ilOfMethod.Emit(OpCodes.Ldind_I4);
                        }
                        if (parameterTypes[i].GetElementType().IsValueType)
                        {
                            ilOfMethod.Emit(OpCodes.Box, parameterTypes[i].GetElementType());
                        }
                    }
                    else
                    {
                        ilOfMethod.Emit(OpCodes.Ldarg, i + 1);
                        if (parameterTypes[i].IsValueType)
                        {
                            ilOfMethod.Emit(OpCodes.Box, parameterTypes[i]);
                        }
                    }
                    ilOfMethod.Emit(OpCodes.Stelem_Ref);

                    ilOfMethod.Emit(OpCodes.Ldloc, pts);
                    ilOfMethod.Emit(OpCodes.Ldc_I4, i);
                    ilOfMethod.Emit(OpCodes.Ldtoken, parameterTypes[i].GetUnderType());
                    ilOfMethod.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
                    if (parameterTypes[i].IsByRef)
                    {
                        ilOfMethod.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("MakeByRefType"));
                    }
                    ilOfMethod.Emit(OpCodes.Stelem_Ref);
                }

                ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                ilOfMethod.Emit(OpCodes.Ldloc, pts);

                LocalBuilder lcReturn = null;
                if (method.ReturnType != typeof(void))
                {
                    lcReturn = ilOfMethod.DeclareLocal(method.ReturnType);
                }

                // call Invoke() method of Interceptor
                ilOfMethod.Emit(OpCodes.Call, typeof(Interceptor).GetMethod("Invoke"));

                // pop the stack if return void
                if (method.ReturnType != typeof(void))
                {
                    ilOfMethod.Emit(OpCodes.Unbox_Any, method.ReturnType);
                    ilOfMethod.Emit(OpCodes.Stloc, lcReturn);
                }
                else
                {
                    ilOfMethod.Emit(OpCodes.Pop);
                }

                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameterTypes[i].IsByRef)
                    {
                        ilOfMethod.Emit(OpCodes.Ldarg, i + 1);
                        ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                        ilOfMethod.Emit(OpCodes.Ldc_I4, i);
                        ilOfMethod.Emit(OpCodes.Ldelem_Ref);
                        if (parameterTypes[i].GetElementType().IsValueType)
                        {
                            ilOfMethod.Emit(OpCodes.Unbox_Any, parameterTypes[i].GetElementType());
                        }
                        ilOfMethod.Emit(OpCodes.Stind_Ref);
                    }
                }

                if (method.ReturnType != typeof(void))
                {
                    ilOfMethod.Emit(OpCodes.Ldloc, lcReturn);
                }
                // complete
                ilOfMethod.Emit(OpCodes.Ret);
            }
        }

        private static FieldInfo BuildConstructor(Type innerType, Type baseType, TypeBuilder typeBuilder)
        {
            // ---- define fields ----

            var fieldCore = typeBuilder.DefineField("_core", baseType, FieldAttributes.Private);
            foreach (var ctor in innerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var parameterTypes = ctor.GetParameters().Select(u => u.ParameterType).ToArray();
                var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);

                ILGenerator il = ctorBuilder.GetILGenerator();
                //for (int i = 0; i <= parameterTypes.Length; i++)
                //{
                //    LoadArgument(il, i);
                //}

                //il.Emit(OpCodes.Call, ctor);

                for (int i = 0; i <= parameterTypes.Length; i++)
                {
                    LoadArgument(il, i);
                }
                il.Emit(OpCodes.Newobj, ctor);

                il.Emit(OpCodes.Stfld, fieldCore);

                il.Emit(OpCodes.Ret);
            }
            return fieldCore;
        }

        private static List<MethodInfo> GetMethods(Type type)
        {
            if (type.IsInterface)
            {
                var mtds = type.GetMethods().ToList();

                var baseIts = type.GetInterfaces();
                foreach (var it in baseIts)
                {
                    mtds.AddRange(GetMethods(it));
                }

                var list = mtds.DistinctBy(info => new { Sign = info.GetSignName() }).ToList();
                return list;
            }
            else
            {
                var mtds = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                return mtds.Where(x => !x.IsAbstract && x.DeclaringType != typeof(object)).ToList();
            }
        }

        public static void LoadArgument(ILGenerator il, int index)
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
        public static void LoadDefaultValue(ILGenerator il, Type type)
        {
            if (type == typeof(string))
            {
                il.Emit(OpCodes.Ldstr, "");
            }
            else if (type.IsValueType)
            {
                var local = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldloca_S, local);
                il.Emit(OpCodes.Initobj, type);
                il.Emit(OpCodes.Ldloc, local);
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
        }
    }
}