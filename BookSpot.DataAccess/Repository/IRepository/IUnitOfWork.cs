using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSpot.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository category { get; } 

        IProductRepository product { get; }

        ICompanyRepository company { get; }

        IShoppingCartRepository shoppingcart { get; }

        IApplicationUserRepository applicationuser { get; }

        IOrderHeaderRepository orderheader { get;}

        IOrderDetailRepository orderdetail { get; }

        void Save();
    }
}
