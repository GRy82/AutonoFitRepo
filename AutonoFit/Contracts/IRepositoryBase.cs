using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IRepositoryBase<T>
    {
        //represents the response of a query
        IQueryable<T> FindAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression); //allows us to pass in linq statements
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
