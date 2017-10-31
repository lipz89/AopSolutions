using System;
using System.Collections.Generic;

namespace AopDecorator
{
    /// <summary>
    /// 相等比较器
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    public class XEqualityComparer<T> : EqualityComparer<T>
    {
        private readonly Func<T, dynamic> keySelector;
        private XEqualityComparer(Func<T, dynamic> keySelector)
        {
            this.keySelector = keySelector;
        }
        public override bool Equals(T x, T y)
        {
            if (x != null)
            {
                return y != null && GetHashCode(x) == GetHashCode(y);
            }
            return y == null;
        }

        public override int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return -1;
            }
            var ok = keySelector?.Invoke(obj);
            if (ok == null)
            {
                return 0;
            }
            return ok.GetHashCode();
        }

        //bool IEqualityComparer.Equals(object x, object y)
        //{
        //    return Equals(x as T, y as T);
        //}

        //int IEqualityComparer.GetHashCode(object obj)
        //{
        //    return GetHashCode(obj as T);
        //}

        /// <summary>
        /// 使用选择器构建一个<typeparamref name="T"/>实例的比较器。
        /// </summary>
        /// <param name="keySelector">如果选择器为空,则返回一个默认比较器</param>
        /// <returns></returns>
        public static IEqualityComparer<T> Get(Func<T, dynamic> keySelector)
        {
            if (keySelector == null)
            {
                return Default;
            }
            return new XEqualityComparer<T>(keySelector);
        }
    }
}