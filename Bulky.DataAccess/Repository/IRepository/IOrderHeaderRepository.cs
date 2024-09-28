using BookSpot.Models;

namespace BookSpot.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);

        void UpdateStatus(int id, string OrderStatus, string? PaymentStatus);
        void UpdateSessionId(int id,string SessionId, string paymentIntentId);

    }
}
