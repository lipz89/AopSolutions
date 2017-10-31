using System;
using System.Reflection;
using System.Threading;

using AopWrapper.Handlers;

namespace Test
{
    public class AopTestClass
    {
        public AopTestClass()
        {
        }

        [CacheHandler(CacheKey = "TestKey", DurationMinutes = 35)]
        public virtual string TestMethod(string word)
        {
            //throw new Exception("test exception");
            return "Hello: " + word;
        }

        [CacheHandler(CacheKey = "TestKey2", DurationMinutes = 35)]
        public virtual int TestMethod2(int i)
        {
            Thread.Sleep(200);
            //dynamic a = 1;
            //a.Eat();
            return i * i;
        }

        [CacheHandler(CacheKey = "TestKey3", DurationMinutes = 35)]
        public virtual void TestMethod3(int i)
        {
            int result = i * i;
            //Console.WriteLine(result);
        }

        /// <summary>
        /// mock emit code
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual int MockAopTest(BaseHandler[] attrs, int i,MethodInfo mi)
        {
            int result = 0;
            MethodContext context = new MethodContext();
            context.ClassName = "AopTestClass";
            context.MethodName = "TestMethod2";
            context.Executor = this;
            context.Parameters = new object[1];
            context.Parameters[0] = i;
            //context.Processed = false;
            context.ReturnValue = result;
            context.ReturnType = typeof(int);

            foreach (var attr in attrs)
            {
                var key = attr.GetHashCode();
                AspectCache.Add(key, attr);
            }

            foreach (var attr in attrs)
            {
                var key = attr.GetHashCode();
                AspectCache.Get(key)?.BeginInvoke(context);
            }

            //foreach (int t in hs)
            //{
            //    AspectCache.Get(t)?.BeginInvoke(context);
            //}

            try
            {
                result = TestMethod2(i);
                context.ReturnValue = result;

                foreach (var attr in attrs)
                {
                    var key = attr.GetHashCode();
                    AspectCache.Get(key)?.EndInvoke(context);
                }
            }
            catch (Exception ex)
            {
                context.Exception = ex;
                foreach (var attr in attrs)
                {
                    var key = attr.GetHashCode();
                    AspectCache.Get(key)?.OnException(context);
                }
            }

            return result;
        }

        /// <summary>
        /// mock emit code
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual void MockAopTest2(BaseHandler[] attrs, int i)
        {
            MethodContext context = new MethodContext();
            context.ClassName = "AopTestClass";
            context.MethodName = "TestMethod2";
            context.Executor = this;
            context.Parameters = new object[1];
            context.Parameters[0] = i;
            context.ReturnType = typeof (void);
            //context.Processed = false;

            foreach (var attr in attrs)
            {
                var key = attr.GetHashCode();
                AspectCache.Add(key, attr);
            }

            foreach (var attr in attrs)
            {
                var key = attr.GetHashCode();
                AspectCache.Get(key)?.BeginInvoke(context);
            }

            //foreach (int t in hs)
            //{
            //    AspectCache.Get(t)?.BeginInvoke(context);
            //}

            try
            {
                TestMethod3(i);

                foreach (var attr in attrs)
                {
                    var key = attr.GetHashCode();
                    AspectCache.Get(key)?.EndInvoke(context);
                }
            }
            catch (Exception ex)
            {
                context.Exception = ex;
                foreach (var attr in attrs)
                {
                    var key = attr.GetHashCode();
                    AspectCache.Get(key)?.OnException(context);
                }
            }
        }
    }
}