using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson.Interfaces
{
    /// <summary>
    /// ek unhasher that also tracks the unhashed keyset
    /// </summary>
    /// <typeparam name="THashKey"></typeparam>
    public interface IEntityKeyUnhasherWithKeyset<THashKey> : IEntityKeyUnhasher<THashKey> 
        where THashKey : class
    {
        /// <summary>
        /// track the unhashed keyset
        /// </summary>
        ISet<EntityKey> Keyset { get;  }
    }
}
