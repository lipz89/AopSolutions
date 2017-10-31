using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Test
{
    [LogHandler]
    [ExceptionHandler]
    public abstract class BaseService<TModel, TEntity> : IBaseService<TModel>
        where TModel : BaseViewModel, new()
        where TEntity : AggregateRoot
    {
        protected readonly IRepository<TEntity> repository;

        /// <summary> 默认排序字段 </summary>
        protected virtual Expression<Func<TEntity, dynamic>> DefaultSortField { get; } = null;

        /// <summary> 默认排序规则 </summary>
        protected virtual SortOrder DefaultSortOrder { get; } = SortOrder.Descending;

        /// <summary> 关联查询的include表达式 </summary>
        protected virtual Expression<Func<TEntity, dynamic>>[] QueryIncludes { get; } = null;

        protected BaseService(IRepositoryContext context, IRepository<TEntity> repository)
        {
            this.repository = repository;
        }

        /// <summary>取所有数据</summary>
        public virtual async Task<List<UserModel>> GetAll(Expression<Func<TModel, dynamic>> sortField = null, SortOrder sortOrder = SortOrder.Unspecified)
        {
            return await Task.Run<List<UserModel>>(() => new List<UserModel> { new UserModel() { ID = 1 } });
        }


        public virtual List<TModel> Get(Expression<Func<TModel, bool>> filter = null, Expression<Func<TModel, dynamic>> sortField = null, SortOrder sortOrder = SortOrder.Unspecified)
        {
            return new List<TModel>();
        }

        public virtual TModel GetOne(Expression<Func<TModel, bool>> filter = null)
        {
            return new TModel();
        }

        /// <summary>根据ID取数据</summary>
        public virtual TModel GetByID(Guid id)
        {
            return new TModel();
        }

        /// <summary>取分页数据</summary>
        protected virtual async Task<PagedData<TModel>> GetPageCore(int pageIndex, int pageSize,
                                                                    Expression<Func<TModel, bool>> filter = null,
                                                                    IDictionary<Expression<Func<TModel, dynamic>>, SortOrder> sorts = null, params Expression<Func<TEntity, dynamic>>[] includes)
        {
            return new PagedData<TModel>();
        }

        /// <summary>取分页数据</summary>
        public virtual async Task<PagedData<TModel>> GetPage(int pageIndex, int pageSize,
                                                             Expression<Func<TModel, bool>> filter = null,
                                                             Dictionary<Expression<Func<TModel, dynamic>>, SortOrder> sorts = null)
        {
            return await GetPageCore(pageIndex, pageSize, filter, sorts);
        }

        /// <summary>添加一条记录</summary>
        public virtual bool Add(TModel model)
        {
            return true;
        }

        /// <summary>删除一条记录</summary>
        public virtual bool Delete(Guid id)
        {
            return true;
        }

        /// <summary>更新一条记录，需要更新的记录有实体的ID指定,更新内容由指定实体指定</summary>
        /// <remarks>改方法可以关联更新子表，需指定</remarks>
        public virtual bool Update(TModel model)
        {
            return true;
        }

        /// <summary>根据ID更新一条记录，更新的内容由指定的action赋值</summary>
        protected virtual bool Update(Guid id, Action<TEntity> action)
        {
            return true;
        }

        /// <summary>更新一条其他记录</summary>
        protected virtual bool Update<T>(IRepository<T> otherRepository, Guid id, Action<T> action)
            where T : AggregateRoot
        {
            return true;
        }

        /// <summary>根据实体更新记录时，在更新之前索要做的操作，可以在这里重写</summary>
        protected virtual void BeforeUpdate(TEntity domain)
        {

        }

        [ExceptionHandler]
        public virtual bool Test()
        {
            return false;
        }

        public virtual bool Test2()
        {
            return false;
        }
        public virtual Task Test3()
        {
            return Task.Run(() => { });
        }
    }
}