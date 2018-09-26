using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson.Interfaces
{
    /// <summary>
    /// takes a skeleton entity and rehydrates it using .Includes etc
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityRehydrator<TEntity> where TEntity : class
    {
        /// <summary>
        /// Rehydrate Entity
        /// </summary>
        /// <param name="dehydrated">Entity which could have just keys filled</param>
        /// <returns>entity rehydrated from DBMS with .Includes as desired</returns>
        TEntity Rehydrate(TEntity dehydrated);
    }
}
