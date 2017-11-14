using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AopDecorator;

using Test;

using TestDecorator.Services;

using UserService = TestDecorator.Services.UserService;

namespace TestDecorator
{
    class Program
    {
        static void Main(string[] args)
        {
            TestComplex();
            //TestComplex();

            Console.Read();
        }

        private static void TestSimple()
        {
            var p = Proxy.Of<Test1, ITest>();
            p.TestMethod();

            p.Write(false);//false

            int ti = 6;
            p.WriteInt(ti);//6
            p.Write(ref ti);//6,5

            p.Write("test");//test
            int i = 100;
            ti = p.Read(out i);
            Console.WriteLine(ti);//5
            Console.WriteLine(i);//5

            var c = p.Read();
            Console.WriteLine(c);//1
        }

        private static void TestSimple2()
        {
            var p = Proxy.Of<Test2, ITest2>();
            p.Write(3);

            p.Write("1", "2", "3");

            p.Write(p, "333");

            int i = 4, io;
            p.Write(ref i, out io);

            p.WriteInt(3, 4, 5);

            p.TestMethod(4, 5, 6);

            var r = p.Read(2);
            Console.WriteLine(r);

            r = p.Read(3, true, "3333333");
            Console.WriteLine(r);
        }

        private static void TestComplex()
        {
            var t = Proxy.Of<UserService, IUserService>(new RepositoryContext(), new Repository<User>());
            Proxy.Save();
            var a = t.Add(new UserModel());
            var b = t.Delete(Guid.Empty);
            var c = t.GetTask();

            var d = t.GetAll();

            var e = t.Get();

            var f = t.GetByID(Guid.NewGuid());
            var g = t.GetOne();
            var h = t.GetPage(0, 10);
            var i = t.Update(new UserModel());
            var j = t.Test();
            var k = t.Test2();
            var l = t.Test3();
        }

        private static void Test11()
        {
            // Mutable struct used in all the following samples


            //------------- Arrays versus other collections -------------//
            List<Mutable> lm = new List<Mutable> { new Mutable(x: 5, y: 5) };
            lm[0].IncrementX(); // Mutating a copy, lm[0].X is 5
            //lm[0].X++; // Fails to compile. lm[0] is not a lvalue.
            Console.WriteLine(lm[0].X);
        }
        private static void Test22()
        {
            Mutable[] am = new Mutable[] { new Mutable(x: 5, y: 5) };
            am[0].IncrementX(); // Mutating the element. lm[0].X is 6
            Console.WriteLine(am[0].X);
            //am[0].X++; // Ok
            // The same can be achieved if a property returns an element by ref in C# 7

        }
        private static void Test33()
        {
            var b = new B();
            b.M.IncrementX(); // mutates a copy
            b.N.IncrementX(); // mutates a copy
            Console.WriteLine(b.M.X);
            Console.WriteLine(b.N.X);
        }
        private static void Test44()
        {
            var d = new Disposable();
            using (d)
            {
                Console.WriteLine(d);
            }

            Console.WriteLine(d.Disposed); // false
        }
    }
    struct Mutable
    {
        public Mutable(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }
        public void IncrementX() { X++; }
        public int X { get; private set; }
        public int Y { get; set; }
    }
    class B
    {
        public readonly Mutable M = new Mutable(x: 5, y: 5);
        public Mutable N = new Mutable(x: 5, y: 5);
    }
    struct Disposable : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() { Disposed = true; }
    }
}
