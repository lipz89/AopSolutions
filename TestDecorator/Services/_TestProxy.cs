using System;

using AopDecorator;

namespace TestDecorator.Services
{
    public class _TestProxy : ITest
    {
        private ITest _core = new Test1();

        public int Read()
        {
            object[] parameters = new object[0];
            Type[] types = new Type[0];
            return (int)Interceptor.Invoke(this._core, "Read", parameters, types);
        }

        public int Read(out int i)
        {
            object[] parameters = new object[1];
            Type[] types = new Type[1];
            parameters[0] = new int();
            types[0] = typeof(int).MakeByRefType();
            var num = (int)Interceptor.Invoke(this._core, "Read", parameters, types);
            i = (int)parameters[0];
            return num;
        }

        public void TestMethod()
        {
            object[] parameters = new object[0];
            Type[] types = new Type[0];
            Interceptor.Invoke(this._core, "TestMethod", parameters, types);
        }

        public void Write(object obj)
        {
            object[] parameters = new object[1];
            Type[] types = new Type[1];
            parameters[0] = obj;
            types[0] = typeof(object);
            Interceptor.Invoke(this._core, "Write", parameters, types);
        }

        public void Write(string str)
        {
            object[] parameters = new object[1];
            Type[] types = new Type[1];
            parameters[0] = str;
            types[0] = typeof(string);
            Interceptor.Invoke(this._core, "Write", parameters, types);
        }

        public void Write(ref int i)
        {
            object[] parameters = new object[1];
            Type[] types = new Type[1];
            parameters[0] = i;
            types[0] = typeof(int).MakeByRefType();
            Interceptor.Invoke(this._core, "Write", parameters, types);
            i = (int)parameters[0];
        }

        public void WriteInt(int i)
        {
            object[] parameters = new object[1];
            Type[] types = new Type[1];
            parameters[0] = i;
            types[0] = typeof(int);
            Interceptor.Invoke(this._core, "WriteInt", parameters, types);
        }
    }
}