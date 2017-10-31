using System;

namespace AopDecorator
{
    public static class Check
    {
        public static void NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(parameterName, string.Format("{0}不能为空.", parameterName));
        }
        public static void NotEmpty(string value, string parameterName)
        {
            if (value.IsNullOrEmptyOrWhiteSpace())
                throw new ArgumentNullException(parameterName, string.Format("{0}值不能为 null、空或者空白字符串.", parameterName));
        }
    }
}