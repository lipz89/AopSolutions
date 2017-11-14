using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDecorator.Services
{
    public class Test1 : ITest
    {
        public void TestMethod()
        {

        }

        public void WriteInt(int i)
        {
            Console.WriteLine(i);
        }

        public void Write(string str)
        {
            Console.WriteLine(str);
        }

        public void Write(object obj)
        {
            Console.WriteLine(obj);
        }

        public int Read()
        {
            return 1;
        }

        public int Read(out int i)
        {
            i = 5;
            return i;
        }

        public void Write(ref int i)
        {
            Console.WriteLine(i);
            i = 5;
            Console.WriteLine(i);
        }
    }
}
