using BookSpot.Models;

namespace BookSpot.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository: IRepository<Category>
    {
        void Update(Category obj);

    }
}
