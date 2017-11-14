using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

using AopDecorator;

using Test;

using TestDecorator.Handlers;

namespace TestDecorator.Services
{
    public class UserService : BaseService<UserModel, User>, IUserService
    {
        public UserService(IRepositoryContext context, IRepository<User> repository) : base(context, repository)
        {
        }
        public Task GetTask()
        {
            return TaskHelper.Empty();
        }

        [CacheHandler("Users")]
        public override async Task<List<UserModel>> GetAll(Expression<Func<UserModel, dynamic>> sortField = null, SortOrder sortOrder = SortOrder.Unspecified)
        {
            return await base.GetAll(sortField, sortOrder);
        }
    }
}