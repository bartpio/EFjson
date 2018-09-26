using System.Data.Entity.Core;

namespace EFjson.Interfaces
{
    /// <summary>
    /// entity key hasher iface
    /// </summary>
    public interface IEntityKeyUnhasher<THashHey> where THashHey : class
    {
        /// <summary>
        /// compute hash for given entitykey
        /// </summary>
        /// <param name="ek"></param>
        /// <returns></returns>
        EntityKey Unhash(THashHey ek);
    }
}