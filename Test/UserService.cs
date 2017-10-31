using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

using AopWrapper;

namespace Test
{
    public class UserService : BaseService<UserModel, User>, IUserService
    {
        public UserService(IRepositoryContext context, IRepository<User> repository) : base(context, repository)
        {
        }

        [CacheHandler]
        public override bool Add(UserModel model)
        {
            return base.Add(model);
        }

        [CacheHandler]
        public virtual Task GetTask()
        {
            return TaskHelper.Empty();
        }

        [CacheHandler]
        public override async Task<List<UserModel>> GetAll(Expression<Func<UserModel, dynamic>> sortField = null, SortOrder sortOrder = SortOrder.Unspecified)
        {
            return await base.GetAll(sortField, sortOrder);
        }
    }
}