using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace AopDecorator
{
    public static class FastObjectCreator
    {
        private delegate object CreateOjectHandler(object[] parameters);
        private static readonly Hashtable creatorCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// CreateObject
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object CreateObject(Type type, params object[] parameters)
        {
            int token = type.MetadataToken;
            Type[] parameterTypes = GetParameterTypes(ref token, parameters);

            var key = token ^ type.FullName.GetHashCode();

            lock (creatorCache.SyncRoot)
            {
                CreateOjectHandler ctor = creatorCache[key] as CreateOjectHandler;
                if (ctor == null)
                {
                    ctor = CreateHandler(type, parameterTypes);
                    creatorCache.Add(key, ctor);
                }
                return ctor.Invoke(parameters);
            }
        }

        /// <summary>
        /// CreateHandler
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paramsTypes"></param>
        /// <returns></returns>
        private static CreateOjectHandler CreateHandler(Type type, Type[] paramsTypes)
        {
            DynamicMethod method = new DynamicMethod("DynamicCreateObject", typeof(object),
                                                     new Type[] { typeof(object[]) }, typeof(CreateOjectHandler).Module);

            ConstructorInfo constructor = type.GetConstructor(paramsTypes);

            ILGenerator il = method.GetILGenerator();

            for (int i = 0; i < paramsTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);
                if (paramsTypes[i].IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, paramsTypes[i]);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, paramsTypes[i]);
                }
            }
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);

            return (CreateOjectHandler)method.CreateDelegate(typeof(CreateOjectHandler));
        }

        /// <summary>
        /// GetParameterTypes
        /// </summary>
        /// <param name="token"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static Type[] GetParameterTypes(ref int token, params object[] parameters)
        {
            if (parameters == null) return new Type[0];
            Type[] values = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                values[i] = parameters[i].GetType();
                token = token * 13 + values[i].MetadataToken;
            }
            return values;
        }
    }
}