using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AopDecorator
{
    internal static class Extensions
    {
        public static Type GetUnderType(this Type type)
        {
            if (type.IsByRef)
            {
                return type.GetElementType();
            }
            return type;
        }
        public static bool IsNullOrVoid(this Type type)
        {
            return type == null || type == typeof(void);
        }

        public static void ForEachReverse<T>(this List<T> list, Action<T> action)
        {
            if (list != null && action != null)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    action(list[i]);
                }
            }
        }
        public static bool IsNullOrEmptyOrWhiteSpace(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            return false;
        }
        public static bool IsNotNullOrEmptyOrWhiteSpace(this string value)
        {
            return !value.IsNullOrEmptyOrWhiteSpace();
        }

        public static string Left(this string value, int length)
        {
            Check.NotEmpty(value, nameof(value));
            var len = value.Length;
            if (len <= length)
            {
                return value;
            }
            return value.Substring(0, length);
        }

        public static string RemoveRight(this string value, int length)
        {
            Check.NotEmpty(value, nameof(value));
            var len = value.Length;
            if (len <= length)
            {
                return value;
            }
            return value.Left(value.Length - length);
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true;
            return !source.Any();
        }

        public static IEnumerable<TSource> DistinctBy<TSource>(this IEnumerable<TSource> source, Func<TSource, dynamic> keySelector)
        {
            return source.Distinct(XEqualityComparer<TSource>.Get(keySelector));
        }

        #region GetFullName

        // Methods
        private static string ExtractGenericArguments(this IEnumerable<Type> names)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Type type in names)
            {
                if (builder.Length > 1)
                {
                    builder.Append(", ");
                }
                builder.Append(type.GetFullName());
            }
            return builder.ToString();
        }

        private static string ExtractName(string name)
        {
            int length = name.IndexOf("`", StringComparison.Ordinal);
            if (length > 0)
            {
                name = name.Substring(0, length);
            }
            return name;
        }

        public static string GetSignName(this MethodInfo method)
        {
            Check.NotNull(method, nameof(method));
            var sign = method.ReturnType.GetFullName() + " ";
            sign += method.GetFullName();
            var ps = method.GetParameters();
            sign += "(";
            if (ps.Any())
            {
                foreach (var info in ps)
                {
                    if (info.ParameterType.IsByRef)
                    {
                        if (info.Attributes.HasFlag(ParameterAttributes.Out))
                        {
                            sign += "out ";
                        }
                        else
                        {
                            sign += "ref ";
                        }
                    }
                    else if (info.IsOptional)
                    {
                        sign += "[Optional]";
                    }
                    sign += info.ParameterType.GetFullName();
                    sign += ", ";
                }
                sign = sign.RemoveRight(1);
            }
            sign += ")";
            return sign;
        }

        public static string GetFullName(this MethodInfo method)
        {
            Check.NotNull(method, nameof(method));

            if (!method.IsGenericMethod)
            {
                return method.Name;
            }
            return ExtractName(method.Name) + "<" + ExtractGenericArguments(method.GetGenericArguments()) + ">";
        }
        private static string GetFullNameCore(this Type type)
        {
            Check.NotNull(type, nameof(type));

            if (type.IsArray)
            {
                var n = type.Name;
                var etype = type.GetElementType();
                return n.Replace(etype.Name, etype.GetFullName());
            }
            var name = type.FullName ?? type.Name;
            if (type.IsGenericType)
            {
                var gtp = ExtractGenericArguments(type.GetGenericArguments());
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return gtp + "?";
                }
                var gt = ExtractName(name);
                return gt + "<" + gtp + ">";
            }
            if (type.IsByRef)
            {
                return name.RemoveRight(1);
            }
            return name;
        }

        public static string GetGenericName(this Type type)
        {
            Check.NotNull(type, nameof(type));

            if (type.IsGenericType)
            {
                return ExtractName(type.Name);
            }
            return string.Empty;
        }

        private static Dictionary<Type, string> simpleName = new Dictionary<Type, string>
        {
            { typeof(object), "object"},
            { typeof(string), "string"},
            { typeof(bool), "bool"},
            { typeof(char) ,"char"},
            { typeof(int), "int"},
            { typeof(uint), "uint"},
            { typeof(byte), "byte"},
            { typeof(sbyte), "sbyte"},
            { typeof(short), "short"},
            { typeof(ushort), "ushort"},
            { typeof(long), "long"},
            { typeof(ulong), "ulong"},
            { typeof(float), "float"},
            { typeof(double), "double"},
            { typeof(decimal), "decimal"}
        };

        public static string GetFullName(this Type type)
        {
            Check.NotNull(type, nameof(type));

            if (simpleName.ContainsKey(type))
            {
                return simpleName[type];
            }
            return type.GetFullNameCore();
        }

        #endregion


        public static string AllMessage(this Exception ex)
        {
            var msg = string.Empty;
            while (ex != null)
            {
                msg += ex.Message + Environment.NewLine;
                ex = ex.InnerException;
            }
            return msg;
        }
    }
}