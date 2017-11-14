using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using AopWrapper.Handlers;

namespace AopWrapper.Aop
{
    /// <summary>
    /// 通过装饰者模式实现AOP
    /// 本类库通过生成代理类的方式实现AOP，因此需要被代理的类要遵守一些约定
    /// 1，被代理的类必须是可被继承的类
    /// 2，类中被继承的方法必须可被重写
    /// 3，没标记特性的方法不会被重写
    /// </summary>
    public static class AOPFactory
    {
        private const string ASSEMBLY_NAME = "AopWrapperDynamicAssembly";
        private const string MODULE_NAME = "AopWrapperDynamicModule";
        private const string TYPE_NAME = "Aop_";

        private static readonly Hashtable typeCache = Hashtable.Synchronized(new Hashtable());
        private static readonly AssemblyBuilder assembly;
        private static readonly ModuleBuilder module;
        static AOPFactory()
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

        /// <summary>
        /// 包装T类型的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns>包装后的对象</returns>
        public static T CreateInstance<T>(params object[] parameters)
            where T : class
        {
            Type baseType = typeof(T);
            Type proxyType = BuilderType(baseType);

            return (T)FastObjectCreator.CreateObject(proxyType, parameters);
        }
        /// <summary>
        /// 包装T类型的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns>包装后的对象</returns>
        public static TI CreateInstance<T, TI>(params object[] parameters)
            where T : class, TI
            where TI : class
        {
            Type baseType = typeof(T);
            Type proxyType = BuilderType(baseType, typeof(TI));

            return (TI)FastObjectCreator.CreateObject(proxyType, parameters);
        }


        #region build proxy type

        #region BuilderType

        public static Type BuilderType(Type baseType, Type interfaceType = null)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            if (interfaceType == null)
            {
                interfaceType = baseType;
            }
            else if (!interfaceType.IsAssignableFrom(baseType))
            {
                throw new Exception("类型 " + interfaceType.Name + " 不能从类型 " + baseType.Name + " 分配实例。");
            }

            //if (!interfaceType.IsInterface)
            //{
            //    var ctor = interfaceType.GetConstructor(Type.EmptyTypes);
            //    if (ctor == null)
            //    {
            //        throw new Exception("指定的类型 " + interfaceType.Name + " 没有无参的构造函数。");
            //    }
            //}

            var key = baseType.AssemblyQualifiedName + "_" + interfaceType.AssemblyQualifiedName;
            Type proxyType = typeCache[key] as Type;

            if (proxyType == null)
            {
                lock (typeCache.SyncRoot)
                {
                    proxyType = BuilderTypeCore(baseType, interfaceType);
                    typeCache.Add(key, proxyType);
                }
            }
            return proxyType;
        }
        private static Type BuilderTypeCore(Type baseType, Type interfaceType)
        {
            string nameOfType = TYPE_NAME + baseType.Name;
            if (interfaceType != baseType)
            {
                nameOfType += "_" + interfaceType.Name;
            }
            TypeBuilder typeBuilder;
            if (interfaceType.IsInterface)
            {
                typeBuilder = module.DefineType(nameOfType, TypeAttributes.Public | TypeAttributes.Sealed);
                typeBuilder.AddInterfaceImplementation(interfaceType);
            }
            else
            {
                typeBuilder = module.DefineType(nameOfType, TypeAttributes.Public | TypeAttributes.Sealed, interfaceType);
            }
            if (baseType.ContainsGenericParameters)
            {
                var gps = baseType.GetGenericArguments().Select(x => x.Name).ToArray();
                if (gps.Any())
                {
                    typeBuilder.DefineGenericParameters(gps);
                }
            }
            var fieldCore = BuildConstructor(typeBuilder, baseType, interfaceType);

            BuildMethod(typeBuilder, baseType, interfaceType, fieldCore);

            Type type = typeBuilder.CreateType();
            return type;
        }
        #endregion

        #region BuildConstructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="interfaceType"></param>
        /// <param name="typeBuilder"></param>
        private static FieldInfo BuildConstructor(TypeBuilder typeBuilder, Type baseType, Type interfaceType)
        {
            if (interfaceType.IsInterface)
            {
                var fieldCore = typeBuilder.DefineField("_core", interfaceType, FieldAttributes.Private);
                foreach (var ctor in baseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                {
                    var parameterTypes = ctor.GetParameters().Select(u => u.ParameterType).ToArray();
                    var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);

                    ILGenerator il = ctorBuilder.GetILGenerator();
                    for (int i = 0; i <= parameterTypes.Length; ++i)
                    {
                        LoadArgument(il, i);
                    }

                    il.Emit(OpCodes.Newobj, ctor);
                    il.Emit(OpCodes.Stfld, fieldCore);
                    il.Emit(OpCodes.Ret);
                }

                return fieldCore;
            }
            else
            {
                foreach (var ctor in baseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                {
                    var parameterTypes = ctor.GetParameters().Select(u => u.ParameterType).ToArray();
                    var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);

                    ILGenerator il = ctorBuilder.GetILGenerator();
                    for (int i = 0; i <= parameterTypes.Length; ++i)
                    {
                        LoadArgument(il, i);
                    }

                    il.Emit(OpCodes.Call, ctor);
                    il.Emit(OpCodes.Ret);
                }

                return null;
            }
        }
        #endregion

        #region GetHandlers

        private static List<int> GetHandlerKeys(MethodInfo method, Type innerType, out bool hasExcpetions)
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
            hanlders = hanlders.OrderBy(x => x.Order).ToList();
            var keys = new List<int>();
            foreach (var attr in hanlders)
            {
                var type = attr.GetType();
                if (type.GetMethod("OnException").DeclaringType == type)
                {
                    hasExcpetions = true;
                }
                var key = attr.GetHashCode();
                HandlerCache.Add(key, attr);
                keys.Add(key);
            }

            return keys;
        }
        private static List<string> GetHandlerJsons(MethodInfo method, Type innerType, out bool hasExcpetions)
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

            hanlders = hanlders.OrderBy(x => x.Order).ToList();
            var keys = new List<string>();
            foreach (var attr in hanlders)
            {
                var type = attr.GetType();
                if (type.GetMethod("OnException").DeclaringType == type)
                {
                    hasExcpetions = true;
                }
                keys.Add(attr.Serialize());
            }

            return keys;
        }
        private static List<IAopHandler> GetHandlers(MethodInfo method, Type innerType, out bool hasExcpetions)
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
                var type = attr.GetType();
                if (type.GetMethod("OnException").DeclaringType == type)
                {
                    hasExcpetions = true;
                }
                var key = attr.GetType().GetHashCode() ^ attr.GetHashCode();
                HandlerCache.Add(key, attr);
            }

            return hanlders.OrderBy(x => x.Order).OfType<IAopHandler>().ToList();
        }

        #endregion

        #region BuildMethod

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

                return mtds.Where(x => !x.IsAbstract && !x.IsSpecialName && x.DeclaringType != typeof(object)).ToList();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="typeBuilder"></param>
        private static void BuildMethod(TypeBuilder typeBuilder, Type baseType, Type interfaceType, FieldInfo fieldCore)
        {
            var methods = GetMethods(interfaceType);
            foreach (var methodInfo in methods)
            {
                #region 方法重写条件

                if (methodInfo == null)
                {
                    continue;
                }

                if (fieldCore == null)
                {
                    var mthdAttrs = methodInfo.Attributes;
                    if (mthdAttrs.HasFlag(MethodAttributes.Final))
                    {
                        continue;
                    }
                }

                bool hasExcpetions;
                var attrs = GetHandlerJsons(methodInfo, baseType, out hasExcpetions);
                if (fieldCore == null && attrs.Count == 0)
                {
                    continue;
                }

                #endregion

                MethodCreator.Create(typeBuilder, baseType, methodInfo, fieldCore, attrs, hasExcpetions);
            }
        }
        #endregion


        #region LoadArgument
        /// <summary>
        /// LoadParameter
        /// </summary>
        /// <param name="il"></param>
        /// <param name="index"></param>
        private static void LoadArgument(ILGenerator il, int index)
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

        #endregion
    }
}
