namespace TestDecorator.Services
{
    public interface ITest
    {
        int Read();
        int Read(out int i);
        void TestMethod();
        void Write(string str);
        void Write(ref int i);
        void Write(object obj);
        void WriteInt(int i);
    }
}