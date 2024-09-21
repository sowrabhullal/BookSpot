using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository
{
    //<T> can be any class, Repo pattern is mainly used when we implement CRUD operations.
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeproperties = null);
        T Get(Expression<Func<T,bool>> filter, string? includeproperties = null);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        //Update is complicated, so its better to not have update as a common method in interface.
        //void Update(T entity);
    }
}
