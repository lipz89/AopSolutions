using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AopWrapper.Aop;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = AOPFactory.CreateInstance<Test, ITest>();
            var test2 = AOPFactory.CreateInstance<Test>();

            var t = test.GetAll<int>(null);

            Console.WriteLine(t.Result);
            test.Write();

            int i = 5;
            test.Write(i);
            Console.WriteLine(i);


            string s = "1";
            test.Write(ref s);
            Console.WriteLine(s);

            test.Write2(out s);
            Console.WriteLine(s);

            test.Write(out i);
            Console.WriteLine(i);

            test.Write2222(out i);
            Console.WriteLine(i);

            object o = false;
            test.Write(out o);
            Console.WriteLine(o);


            var userSvc = AOPFactory.CreateInstance<UserService, IUserService>(new RepositoryContext(), new Repository<User>());
            var flag = userSvc.Delete(Guid.NewGuid());
            var model = userSvc.GetAll();
            var a = userSvc.Add(new UserModel());
            var b = userSvc.Get();
            var c = userSvc.GetByID(Guid.NewGuid());
            var d = userSvc.GetOne();
            var e = userSvc.GetPage(1, 10);
            var f = userSvc.Update(new UserModel());

            Console.WriteLine(model.Result);
            Console.WriteLine(e.Result);


            AOPFactory.Save();
            Console.Read();
        }

        private static void TestTypeCode()
        {
            var types = new Type[] { typeof(bool), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(char), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(decimal), typeof(double), typeof(float), typeof(IntPtr), typeof(UIntPtr) };

            foreach (var type in types)
            {
                Console.WriteLine("{0} : {1}", type.FullName, Type.GetTypeCode(type));
            }
        }
    }

    class TestDynamic
    {
        public static IQueryable<Model> GetQueryable()
        {
            var list = new List<Model> { new Model() { ID = 1, Name = "Name1" }, new Model() { Name = "Name2", ID = 2 } }.AsQueryable();
            return list;
        }
    }

    class Model
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            //return false;
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            Model iAggregateRoot = obj as Model;
            if (iAggregateRoot == null)
                return false;

            return this.ID.Equals(iAggregateRoot.ID);
        }

        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        /// <returns>当前Object的哈希代码。</returns>
        /// <remarks>有关此函数的更多信息，请参见：http://msdn.microsoft.com/zh-cn/library/system.object.gethashcode。
        /// </remarks>
        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(Model left, Model right)
        {
            if (Equals(left, null))
            {
                return Equals(right, null);
            }

            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(Model left, Model right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{GetType().Name} {ID}]";
        }
    }

    public class TestEqualityComparer
    {
        public static void Test()
        {
            var list = new List<Model>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new Model() { ID = i, Name = "10" });
            }

            for (int i = 0; i < 5; i++)
            {
                list.Add(new Model() { ID = i, Name = "5" });
            }

            Console.WriteLine(list.Count);
            var ds = list.Distinct();
            Console.WriteLine(ds.Count());
        }
    }
}


