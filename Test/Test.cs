using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    [LogHandler]
    [ExceptionHandler]
    public class Test :  ITest
    {
        public void TestBool()
        {
            var b = false;
            TestBoolMethod(b);

            TestBoolMethod(true);

            TestBoolMethod(false);
        }
        public void TestBoolMethod(bool b)
        {
            Console.WriteLine(b);
        }
        public virtual void Write()
        {
            Console.WriteLine("write");
        }

        public virtual void Write(string str)
        {
            Console.WriteLine("write:" + str);
        }

        public virtual void Write2(out string str)
        {
            str = ".....";
            //object arg_46_0 = this._core;
            //string arg_46_1 = "Write";
            //object[] array = new object[1];
            //Type[] array2 = new Type[1];
            //array[0] = default(string);
            //array2[0] = typeof(string).MakeByRefType();
            //Interceptor.Invoke(arg_46_0, arg_46_1, array, array2);
            //str = (string)array[0];
        }

        public virtual void Write(out int str)
        {
            str = 15;
            //var arr = new object[] { str };
            //var m = typeof(Test).GetMethod("Write2222");
            //m.Invoke(this, arr);
            //str = (int)arr[0];
        }
        public virtual void Write2222(out int str)
        {
            str = 100;
        }

        public virtual void Write(ref string str)
        {
            str = "111";
            //var arr = new object[] { str };
            //str = (string)arr[0];
        }

        public virtual void Write(out object str)
        {
            str = "object";
        }

        public virtual void Write(params object[] args)
        {
            Console.WriteLine("write:");
            foreach (var o in args)
            {
                Console.WriteLine(o);
            }
        }

        public virtual string Read()
        {
            return "read";
        }

        [CacheHandler(CacheKey = "Read")]
        public virtual int Read(int a, int b)
        {
            return a + b;
        }
        [CacheHandler(CacheKey = "GetAll")]
        public virtual async Task<List<TModel>> GetAll<TModel>(Expression<Func<TModel, dynamic>> sortField = null, SortOrder? sortOrder = null)
        {
            return await Task.Run<List<TModel>>(() =>
                                                {
                                                    Console.WriteLine("5秒后执行getall");
                                                    Thread.Sleep(5000);
                                                    Console.WriteLine("现在执行getall");
                                                    return new List<TModel>(-1);
                                                });
        }
    }


    public class Test2 : Test
    {
        private object _core = new Test();
        public void Test<T>()
        {
            var d = default(T);
            Console.WriteLine(d);
        }

        //public override void Write(out int str)
        //{
        //    object arg_46_0 = this._core;
        //    string arg_46_1 = "Write";
        //    object[] array = new object[1];
        //    Type[] array2 = new Type[1];
        //    array[0] = default(int);
        //    array2[0] = typeof(int).MakeByRefType();
        //    Interceptor.Invoke(arg_46_0, arg_46_1, array, array2);
        //    str = (int)array[0];
        //}
    }
}