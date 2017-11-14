using System.Data.SqlClient;

namespace Test
{
    public interface ITest
    {
        System.Threading.Tasks.Task<System.Collections.Generic.List<TModel>> GetAll<TModel>(System.Linq.Expressions.Expression<System.Func<TModel, dynamic>> sortField = null, SortOrder? sortOrder = default(SortOrder?));
        string Read();
        int Read(int a, int b);
        void TestBool();
        void TestBoolMethod(bool b);
        void Write();
        void Write(ref string str);
        void Write(string str);
        void Write(params object[] args);
        void Write(out object str);
        void Write(out int str);
        void Write2(out string str);
        void Write2222(out int str);
    }
}