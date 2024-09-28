using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookSpot.DataAccess.Data;
using BookSpot.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookSpot.DataAccess.Repository
{
    public class Repository<T>: IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _dbSet;
        public Repository(ApplicationDbContext db) {
            _db = db;
            _dbSet = db.Set<T>();
            _db.Products.Include(u => u.Category).Include(u => u.CategoryId);
        }
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeproperties = null, bool track=false)
        {
            //Asnotracting will make sure the db is not updated when we get the item from database
            IQueryable<T> query;

            if (track == true)
            {
                query = _dbSet;
            }
            else {
                query = _dbSet.AsNoTracking();
            }

            if (includeproperties != null)
            {
                foreach (var property in includeproperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        //must handle multiple includes
        //includeproperties is to handle foreign key relationship, ie to get the values of
        //category table as part of the product table
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeproperties=null, bool track=false)
        {
            IQueryable<T> query;

            if (track == true)
            {
                query = _dbSet;
            }
            else
            {
                query = _dbSet.AsNoTracking();
            }

            if (includeproperties != null) {
                foreach (var property in includeproperties
                    .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                { 
                    query=query.Include(property);
                }
            }
            if (filter != null) {
                query = query.Where(filter);
            }

            return query.ToList();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
