using BookSpot.DataAccess.Data;
using BookSpot.DataAccess.Repository.IRepository;
using BookSpot.Models;

namespace BookSpot.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdateSessionId(int id, string SessionId, string paymentIntentId)
        {
            OrderHeader obj = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

            if (SessionId!=null)
            {
                obj.SessionId = SessionId;   
            }

            if (paymentIntentId!=null) { 
                obj.PaymentIntentId = paymentIntentId;
                obj.PaymentDate = DateTime.Now;
            }

            obj.SessionId = SessionId;
            obj.PaymentIntentId = paymentIntentId;

            //_db.OrderHeaders.Update(obj);
        }
        public void UpdateStatus(int id, string OrderStatus, string? PaymentStatus)
        {
            OrderHeader obj = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (obj != null)
            {
                obj.OrderStatus = OrderStatus;
                if (PaymentStatus != null)
                {
                    obj.PaymentStatus = PaymentStatus;
                }
            }
            //_db.OrderHeaders.Update(obj);
        }
    }
}
