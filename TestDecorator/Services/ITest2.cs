using System.Collections.Generic;
using Test;

namespace TestDecorator.Services
{
    public interface ITest2
    {
        int Read(int a = 1, bool b = false, string c = "c", object d = null, List<User> users = null);
        void TestMethod(params int[] ints);
        void Write(params string[] str);
        void Write(object obj, string str = "222");
        void Write(ref int i, out int io, string k = null, params object[] objs);
        void WriteInt(int i, int i2, int i3 = 10);
    }
}