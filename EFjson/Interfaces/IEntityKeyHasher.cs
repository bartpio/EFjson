using System.Data.Entity.Core;

namespace EFjson.Interfaces
{
    /// <summary>
    /// entity key hasher iface
    /// </summary>
    public interface IEntityKeyHasher<THashHey> where THashHey : class
    {
        /// <summary>
        /// compute hash for given entitykey
        /// </summary>
        /// <param name="ek"></param>
        /// <returns></returns>
        THashHey ComputeHash(EntityKey ek);
    }
}