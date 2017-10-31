using System;

namespace AopWrapper.Handlers
{
    public class Parameter
    {
        public Type Type { get;internal set; }
        public string Name { get; internal set; }
        public object Value { get; internal set; }
    }
}