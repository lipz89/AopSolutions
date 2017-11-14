using System;
using System.Collections.Generic;

using Test;

namespace TestDecorator.Services
{
    public class Test2 : ITest2
    {
        public void TestMethod(params int[] ints)
        {
            foreach (var i in ints)
            {
                Console.Write(i + " ,");
            }
        }

        public void WriteInt(int i, int i2, int i3 = 10)
        {
            Console.WriteLine(i + " " + i2 + " " + i3);
        }

        public void Write(params string[] str)
        {
            foreach (var i in str)
            {
                Console.Write(i + " ,");
            }
        }

        public void Write(object obj, string str = "222")
        {
            Console.WriteLine(obj + " - " + str);
        }

        public int Read(int a = 1, bool b = false, string c = "c", object d = null, List<User> users = null)
        {
            Console.WriteLine(a + " - " + b + " - " + c + " - " + d);
            return a;
        }

        public void Write(ref int i, out int io, string k = null, params object[] objs)
        {
            io = 5;
            Console.WriteLine(i);
            i = 5;
            Console.WriteLine(i);
        }
    }
}