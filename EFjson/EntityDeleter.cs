using EFjson.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson
{
    /// <summary>
    /// Entity deleter
    /// Use together with SerializationSession, in Deserialize mode, to delete Entities that were not present in the payload submitted by user
    /// </summary>
    /// <remarks>
    /// Somewhat experimental; use with care
    /// </remarks>
    public class EntityDeleter : IHasDbContext
    {
        /// <summary>
        /// our db context
        /// </summary>
        public DbContext MyDbContext { get; }

        /// <summary>
        /// keyset storage
        /// </summary>
        private readonly ISet<EntityKey> _keyset;

        /// <summary>
        /// entity deleter cons
        /// </summary>
        /// <param name="dbc">db context</param>
        /// <param name="keyset">Keyset from SerializationSession (in deserialize mode); AFTER its .Deserialize has been called. Pass with care; do not pass a random or newly instantiated empty set</param>
        public EntityDeleter(DbContext dbc, ISet<EntityKey> keyset)
        {
            MyDbContext = dbc;
            _keyset = keyset;
        }

        /// <summary>
        /// Mark deleted entries
        /// </summary>
        /// <returns>The entries that were marked deleted.</returns>
        public IReadOnlyList<ObjectStateEntry> MarkDeletedEntities()
        {
            var oc = this.GetObjectContext();
            //now we do a bit of a trick to detect deletes
            oc.DetectChanges(); //it's critical.
            var supposeds = oc.ObjectStateManager.GetObjectStateEntries(EntityState.Modified).ToImmutableList();
            var supposet = supposeds.ToImmutableHashSet();

            var pd = supposeds.Select(x => new { x, modded = x.GetModifiedProperties().ToImmutableList() }).ToImmutableArray();  //for debug.
            var todelete = supposet.Where(x => !_keyset.Contains(x.EntityKey)).ToImmutableList();
            foreach (var todel in todelete)
            {
                todel.Delete();
            }

            return todelete;
        }
    }
}
