namespace Test
{
    public interface IRepository<T> where T : AggregateRoot
    {
    }

    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
    }

    public interface IRepositoryContext
    {
    }

    public class RepositoryContext : IRepositoryContext
    {
    }

    public class PagedData<T>
    {
    }

    public class BaseViewModel
    {
    }

    public class AggregateRoot
    {
    }

    public class User : AggregateRoot
    {
    }

    public class UserModel : BaseViewModel
    {
        public int ID { get; set; }
    }

}