using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Test
{
    public interface IUserService : IBaseService<UserModel>
    {
        bool Add(UserModel model);
        new bool Delete(Guid id);

        Task GetTask();
    }
    public interface IBaseService<TModel> where TModel : BaseViewModel, new()
    {
        bool Add(TModel model);
        bool Delete(Guid id);
        List<TModel> Get(Expression<Func<TModel, bool>> filter = null, Expression<Func<TModel, dynamic>> sortField = null, SortOrder sortOrder = SortOrder.Unspecified);
        Task<List<UserModel>> GetAll(Expression<Func<TModel, dynamic>> sortField = null, SortOrder sortOrder = SortOrder.Unspecified);
        TModel GetByID(Guid id);
        TModel GetOne(Expression<Func<TModel, bool>> filter = null);
        Task<PagedData<TModel>> GetPage(int pageIndex, int pageSize, Expression<Func<TModel, bool>> filter = null, Dictionary<Expression<Func<TModel, dynamic>>, SortOrder> sorts = null);
        bool Test();
        bool Test2();
        Task Test3();
        bool Update(TModel model);
    }
}