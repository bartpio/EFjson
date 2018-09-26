using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.Data.Entity.Core;
using Newtonsoft.Json;
using System.IO;
using EFjson.Interfaces;

namespace EFjson.HashingBase.SerializationInternals
{
    /// <summary>
    /// helper to add support for deser
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="THashKey"></typeparam>
    internal class DeserializationSession<TEntity, THashKey> : IHasDbContext, IEntityKeyUnhasherWithKeyset<THashKey> where TEntity : class
        where THashKey : class
    {
        /// <summary>
        /// entity key hasher
        /// </summary>
        private readonly IEntityKeyHasher<THashKey> _iekh;

        /// <summary>
        /// unhasher storage
        /// </summary>
        private readonly Dictionary<THashKey, EntityKey> _unhasher = new Dictionary<THashKey, EntityKey>();

        /// <summary>
        /// A new set of keys.
        /// </summary>
        private readonly ISet<EntityKey> _keyset = new HashSet<EntityKey>();

        /// <summary>
        /// keyset
        /// </summary>
        public ISet<EntityKey> Keyset => _keyset;

        /// <summary>
        /// for json ser. custom configged.
        /// </summary>
        private readonly JsonSerializer _js;

        /// <summary>
        /// rehydrator
        /// </summary>
        private readonly IEntityRehydrator<TEntity> _rehydrator;

        /// <summary>
        /// cons
        /// </summary>
        /// <param name="dbc"></param>
        /// <param name="iekh"></param>
        /// <param name="rehydrator"></param>
        /// <param name="_js"></param>
        public DeserializationSession(DbContext dbc, IEntityKeyHasher<THashKey> iekh, IEntityRehydrator<TEntity> rehydrator, JsonSerializer js)
        {
            MyDbContext = dbc;
            _iekh = iekh;
            _rehydrator = rehydrator;
            _js = js;
        }

        /// <summary>
        /// we track db context
        /// </summary>
        public DbContext MyDbContext { get; }


        /// <summary>
        /// obj materialized evt handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Oc_ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
        {
            var oc = this.GetObjectContext();
            var se = oc.ObjectStateManager.GetObjectStateEntry(e.Entity);
            var ek = se.EntityKey;
            _unhasher[_iekh.ComputeHash(ek)] = ek;
        }

        /// <summary>
        /// unhash.
        /// </summary>
        /// <param name="ek"></param>
        /// <returns></returns>
        EntityKey IEntityKeyUnhasher<THashKey>.Unhash(THashKey ek)
        {
            return _unhasher[ek];
        }

        /// <summary>
        /// deserialize; all we generally need here is the root obj
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public virtual TEntity DeserializeRootShell(string str)
        {
            //deserialize, not we do NOT use the main jss settings here... We use special ones and ignore circulars...
            var jss_minimal = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.DeserializeObject<TEntity>(str, jss_minimal);
        }

        /// <summary>
        /// deserialize.
        /// </summary>
        /// <param name="str">str being deserialized</param>
        /// <returns></returns>
        public TEntity Deserialize(string str)
        {
            var oc = this.GetObjectContext();

            var rootShell = DeserializeRootShell(str);
            oc.ObjectMaterialized += Oc_ObjectMaterialized;
            try
            {
                _rehydrator.Rehydrate(rootShell);
            }
            finally
            {
                oc.ObjectMaterialized -= Oc_ObjectMaterialized;
            }

            var jreader = new JsonTextReader(new StringReader(str));
            var result = _js.Deserialize<TEntity>(jreader);
            return result;
        }
    }
}
