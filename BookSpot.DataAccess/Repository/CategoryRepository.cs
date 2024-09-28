using BookSpot.DataAccess.Data;
using BookSpot.DataAccess.Repository.IRepository;
using BookSpot.Models;

namespace BookSpot.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category> ,ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db) { 
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}
