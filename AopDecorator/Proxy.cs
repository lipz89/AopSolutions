using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AopDecorator
{
    public static class Proxy
    {
        private static readonly Hashtable typeCache = Hashtable.Synchronized(new Hashtable());
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
            else
            {
                if (!baseType.IsAssignableFrom(innerType))
                {
                    throw new Exception(string.Format("类型{0}不能从类型{1}分配实例。", baseType, innerType));
                }
            }
            lock (typeCache.SyncRoot)
            {
                Type proxyType = typeCache[baseType] as Type;

                if (proxyType == null)
                {
                    proxyType = Override(innerType, baseType);
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

        private static Type Override(Type innerType, Type baseType)
        {
            string nameOfAssembly = innerType.Name + "ProxyAssembly";
            string nameOfModule = innerType.Name + "ProxyModule";
            string nameOfType = innerType.Name + "Proxy";
            var dllName = nameOfAssembly + ".dll";

            var assemblyName = new AssemblyName(nameOfAssembly);
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assembly.DefineDynamicModule(nameOfModule, dllName);

            TypeBuilder typeBuilder;
            if (baseType.IsInterface)
            {
                typeBuilder = moduleBuilder.DefineType(nameOfType, TypeAttributes.Public);
                typeBuilder.AddInterfaceImplementation(baseType);
            }
            else
            {
                typeBuilder = moduleBuilder.DefineType(nameOfType, TypeAttributes.Public, baseType);
            }

            InjectInterceptor(innerType, baseType, typeBuilder);

            var t = typeBuilder.CreateType();

            //assembly.Save(dllName);

            return t;
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
                for (int i = 0; i <= parameterTypes.Length; i++)
                {
                    LoadArgument(il, i);
                }

                il.Emit(OpCodes.Call, ctor);

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

                var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.Standard, method.ReturnType, parameterTypes);
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

                // create instance of T
                //ilOfMethod.Emit(OpCodes.Newobj, typeof(T).GetConstructor(new Type[0]));
                ilOfMethod.Emit(OpCodes.Ldstr, method.Name);

                // build the method parameters
                //if (methodParameterTypes == null)
                //{
                //    ilOfMethod.Emit(OpCodes.Ldnull);
                //}
                //else
                //{
                var parameters = ilOfMethod.DeclareLocal(typeof(object[]));
                ilOfMethod.Emit(OpCodes.Ldc_I4, methodParameterTypes.Length);
                ilOfMethod.Emit(OpCodes.Newarr, typeof(object));
                ilOfMethod.Emit(OpCodes.Stloc, parameters);

                var pts = ilOfMethod.DeclareLocal(typeof(Type[]));
                ilOfMethod.Emit(OpCodes.Ldc_I4, methodParameterTypes.Length);
                ilOfMethod.Emit(OpCodes.Newarr, typeof(Type));
                ilOfMethod.Emit(OpCodes.Stloc, pts);

                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ilOfMethod.Emit(OpCodes.Ldloc, parameters);
                    ilOfMethod.Emit(OpCodes.Ldc_I4, i);
                    if (parameterTypes[i].IsByRef)
                    {
                        LoadDefaultValue(ilOfMethod, parameterTypes[i].GetElementType());
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
                //}

                // call Invoke() method of Interceptor
                ilOfMethod.Emit(OpCodes.Call, typeof(Interceptor).GetMethod("Invoke"));


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

                // pop the stack if return void
                if (method.ReturnType == typeof(void))
                {
                    ilOfMethod.Emit(OpCodes.Pop);
                }
                else
                {
                    ilOfMethod.Emit(OpCodes.Castclass, method.ReturnType);
                }
                // complete
                ilOfMethod.Emit(OpCodes.Ret);
            }
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