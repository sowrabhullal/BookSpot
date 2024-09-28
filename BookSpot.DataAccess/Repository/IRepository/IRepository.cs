using System.Linq.Expressions;

namespace BookSpot.DataAccess.Repository.IRepository
{
    //<T> can be any class, Repo pattern is mainly used when we implement CRUD operations.
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeproperties = null, bool track=false);
        T Get(Expression<Func<T,bool>> filter, string? includeproperties = null, bool track=false);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        //Update is complicated, so its better to not have update as a common method in interface.
        //void Update(T entity);
    }
}
