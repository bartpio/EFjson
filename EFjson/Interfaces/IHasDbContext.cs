using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson.Interfaces
{
    /// <summary>
    /// simple has db context iface
    /// </summary>
    public interface IHasDbContext
    {
        /// <summary>
        /// provide access to my db context
        /// </summary>
        DbContext MyDbContext { get;  }
    }
}
