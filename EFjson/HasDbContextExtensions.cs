using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using EFjson.Interfaces;

namespace EFjson
{
    /// <summary>
    /// db context extensions
    /// </summary>
    internal static class HasDbContextExtensions
    {
        /// <summary>
        /// pull oc adapter
        /// </summary>
        /// <param name="ihasdbc"></param>
        /// <returns></returns>
        internal static IObjectContextAdapter GetObjectContextAdapter(this IHasDbContext ihasdbc)
        {
            return (IObjectContextAdapter)(ihasdbc.MyDbContext);
        }

        /// <summary>
        /// pull oc
        /// </summary>
        /// <param name="ihasdbc"></param>
        /// <returns></returns>
        internal static ObjectContext GetObjectContext(this IHasDbContext ihasdbc)
        {
            return ihasdbc.GetObjectContextAdapter().ObjectContext;
        }
    }
}
