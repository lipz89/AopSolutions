using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AopDecorator;

namespace Test.TestTask
{
    public class Methods
    {
        public Task Get2()
        {
            return TaskHelper.Empty();
        }

        public async Task Get()
        {
            await TaskHelper.Empty();
        }

        public async Task<int> GetInt()
        {
            return await Task.Run(() => 5);
        }

        public Task<int> GetInt2()
        {
            return Task.Run(() => 5);
        }

        public async Task<int> GetInts(int a, int b)
        {
            return await Task.Run(() => a + b);
        }
    }

    public class TestTask
    {
        private readonly Methods methods = new Methods();

        public async Task Get()
        {
            await methods.Get();
        }

        public async Task Get2()
        {
            await methods.Get2();
        }
    }

    public class TestTask2
    {
        private readonly Methods methods = new Methods();

        public async Task<int> GetInt()
        {
            return await methods.GetInt();
        }

        public async Task<int> GetInt2()
        {
            return await methods.GetInt2();
        }
    }

    public class TestTask3
    {
        private readonly Methods methods = new Methods();

        public async Task<int> GetInts()
        {
            return await methods.GetInts(2, 3);
        }
    }

    public class AsyncWork<T>
    {
        public TaskAwaiter<T> GetAwaiter()
        {
            return new TaskAwaiter<T> { };
        }
    }

    public class TaskBuilder<T, TResult> : IAsyncStateMachine
    {
        private readonly Func<TResult> fun;

        public TaskBuilder(T t, Func<TResult> fun)
        {
            this.fun = fun;
            Builder = AsyncTaskMethodBuilder<TResult>.Create();
            this.Executor = t;
        }

        public T Executor { get; }

        public AsyncTaskMethodBuilder<TResult> Builder { get; }

        public int State { get; set; } = -1;

        private TaskAwaiter<TResult> uu__1;
        private TResult ss__1;

        public void MoveNext()
        {
            int num = this.State;
            TResult num2;
            try
            {
                bool flag = num != 0;
                TaskAwaiter<TResult> awaiter;
                if (flag)
                {
                    awaiter = Task.Run<TResult>(fun).GetAwaiter();
                    bool flag2 = !awaiter.IsCompleted;
                    if (flag2)
                    {
                        this.State = 0;
                        this.uu__1 = awaiter;
                        var stateMachine = this;
                        this.Builder.AwaitUnsafeOnCompleted<TaskAwaiter<TResult>, TaskBuilder<T, TResult>>(ref awaiter, ref stateMachine);
                        return;
                    }
                }
                else
                {
                    awaiter = this.uu__1;
                    this.uu__1 = default(TaskAwaiter<TResult>);
                    this.State = -1;
                }

                TResult result = awaiter.GetResult();
                awaiter = default(TaskAwaiter<TResult>);
                this.ss__1 = result;
                num2 = this.ss__1;
            }
            catch (Exception exception)
            {
                this.State = -2;
                this.Builder.SetException(exception);
                return;
            }

            this.State = -2;
            this.Builder.SetResult(num2);
        }

        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public Task<TResult> Return()
        {
            var b = this;
            this.Builder.Start<TaskBuilder<T, TResult>>(ref b);
            return b.Builder.Task;
        }
    }

    public class TestTask333
    {
        private readonly Methods methods = new Methods();

        [AsyncStateMachine(typeof(TaskBuilder<TestTask333, int>))]
        public Task<int> GetInts()
        {
            try
            {

                var builder = new TaskBuilder<TestTask333, int>(this, () =>
                                                                {
                                                                    Thread.Sleep(1000);
                                                                    throw new Exception("111");

                                                                    return 2 + 3;
                                                                });
                return builder.Return();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Task.FromResult(-1);
            }
        }
    }

    public class Test2323
    {
        public static async Task<int> Test()
        {
            try
            {
                var r = new TestTask333();
                var a =await r.GetInts();
                Console.WriteLine(a);
                return a;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }
    }
}
