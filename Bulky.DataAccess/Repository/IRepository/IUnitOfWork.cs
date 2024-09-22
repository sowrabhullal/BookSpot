﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository category { get; } 

        IProductRepository product { get; }

        ICompanyRepository company { get; }

        void Save();
    }
}
