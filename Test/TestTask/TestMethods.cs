using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using AopWrapper;

namespace Test.TestTask
{
    public class TestMethods
    {
        // TestMethods
        [AsyncStateMachine(typeof (GetD0)), DebuggerStepThrough]
        public Task Get()
        {
            GetD0 stateMachine = new GetD0
            {
                this__4 = this,
                builder__t = AsyncTaskMethodBuilder.Create(),
                state__1 = -1
            };
            stateMachine.builder__t.Start<GetD0>(ref stateMachine);
            return stateMachine.builder__t.Task;
        }

        [AsyncStateMachine(typeof (GetD1)), DebuggerStepThrough]
        public  Task<int> GetInt()
        {
            GetD1 stateMachine = new GetD1
            {
                this__4 = this,
                builder__t = AsyncTaskMethodBuilder<int>.Create(),
                state__1 = -1
            };
            stateMachine.builder__t.Start<GetD1>(ref stateMachine);
            return  stateMachine.builder__t.Task;
        }

        // Nested Types
        [Serializable]
        private sealed class anc
        {
            // Fields
            public static readonly TestMethods.anc anc__9 = new TestMethods.anc();
            public static Func<int> dlg__9;

            // TestMethods
            internal int b__1_0()
            {
                return 5;
            }
        }
        
        private sealed class GetD0 : IAsyncStateMachine
        {
            // Fields
            public int state__1;
            public TestMethods this__4;
            public AsyncTaskMethodBuilder builder__t;
            private TaskAwaiter u__1;

            // TestMethods
            public void MoveNext()
            {
                int num = this.state__1;
                try
                {
                    TaskAwaiter awaiter;
                    if (num != 0)
                    {
                        awaiter = TaskHelper.Empty().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            this.state__1 = num = 0;
                            this.u__1 = awaiter;
                            TestMethods.GetD0 stateMachine = this;
                            this.builder__t.AwaitUnsafeOnCompleted<TaskAwaiter, TestMethods.GetD0>(ref awaiter, ref stateMachine);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = this.u__1;
                        this.u__1 = new TaskAwaiter();
                        this.state__1 = num = -1;
                    }

                    awaiter.GetResult();
                    awaiter = new TaskAwaiter();
                }
                catch (Exception exception)
                {
                    this.state__1 = -2;
                    this.builder__t.SetException(exception);
                    return;
                }

                this.state__1 = -2;
                this.builder__t.SetResult();
            }

            [DebuggerHidden]
            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
            }
        }
        
        private sealed class GetD1 : IAsyncStateMachine
        {
            // Fields
            public int state__1;
            public TestMethods this__4;
            private int ss__1;
            public AsyncTaskMethodBuilder<int> builder__t;
            private TaskAwaiter<int> uu__1;

            // TestMethods
            public void MoveNext()
            {
                int num2;
                int num = this.state__1;
                try
                {
                    TaskAwaiter<int> awaiter;
                    if (num != 0)
                    {
                        awaiter = Task.Run<int>(TestMethods.anc.dlg__9 ?? (TestMethods.anc.dlg__9 = new Func<int>(TestMethods.anc.anc__9.b__1_0))).GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            this.state__1 = num = 0;
                            this.uu__1 = awaiter;
                            TestMethods.GetD1 stateMachine = this;
                            this.builder__t.AwaitUnsafeOnCompleted<TaskAwaiter<int>, TestMethods.GetD1>(ref awaiter, ref stateMachine);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = this.uu__1;
                        this.uu__1 = new TaskAwaiter<int>();
                        this.state__1 = num = -1;
                    }

                    int result = awaiter.GetResult();
                    awaiter = new TaskAwaiter<int>();
                    this.ss__1 = result;
                    num2 = this.ss__1;
                }
                catch (Exception exception)
                {
                    this.state__1 = -2;
                    this.builder__t.SetException(exception);
                    return;
                }

                this.state__1 = -2;
                this.builder__t.SetResult(num2);
            }

            [DebuggerHidden]
            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
            }
        }
    }
}
