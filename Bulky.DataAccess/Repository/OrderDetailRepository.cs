using BookSpot.DataAccess.Data;
using BookSpot.DataAccess.Repository.IRepository;
using BookSpot.Models;

namespace BookSpot.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail> , IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db) { 
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
