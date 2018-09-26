using EFjson.HashingBase.SerializationInternals;
using EFjson.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson
{
    /// <summary>
    /// serialization session
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="THashKey"></typeparam>
    public abstract class SerializationSession<TEntity, THashKey> : IHasDbContext where TEntity : class
        where THashKey : class
    {
        /// <summary>
        /// my db context
        /// </summary>
        public DbContext MyDbContext { get; }

        /// <summary>
        /// entity key hasher
        /// </summary>
        private readonly IEntityKeyHasher<THashKey> _iekh;

        /// <summary>
        /// serializer for the main op.
        /// </summary>
        private readonly JsonSerializer _js;

        /// <summary>
        /// filled only if DESERIALIZATION mode passed in cons.
        /// </summary>
        private readonly DeserializationSession<TEntity, THashKey> _des;

        /// <summary>
        /// this is valid only DESERIALIZATION mode.
        /// </summary>
        public ISet<EntityKey> Keyset => _des.Keyset;

        /// <summary>
        /// rehydrator
        /// </summary>
        private readonly IEntityRehydrator<TEntity> _rehydrator;

        /// <summary>
        /// access to mode
        /// </summary>
        public SerializationSessionMode Mode => _des != null ? SerializationSessionMode.Deserialization : SerializationSessionMode.Serialization;


        /// <summary>
        /// deserializer session cons
        /// </summary>
        /// <param name="dbc">db context</param>
        /// <param name="iekh">key hasher</param>
        /// <param name="js">json serializer to use; may be somewhat mutated. should be devoted to this one operation.</param>
        public SerializationSession(DbContext dbc, IEntityKeyHasher<THashKey> iekh, IEntityRehydrator<TEntity> rehydrator, SerializationSessionMode mode, JsonSerializerSettings jss)
        {
            MyDbContext = dbc;
            _iekh = iekh;
            _rehydrator = rehydrator;


            if (jss.ContractResolver != null)
            {
                throw new ArgumentOutOfRangeException("jss", "the supplied JsonSerializer should not already have a ContractResolver established");
            }
            else
            {
                //expected case.
                _js = JsonSerializer.CreateDefault(jss);
                if (mode == SerializationSessionMode.Deserialization)
                {
                    _des = new DeserializationSession<TEntity, THashKey>(dbc, iekh, _rehydrator, _js);
                }

                _js.ContractResolver = new EntityContractResolver<THashKey>(MyDbContext, _iekh, _des);
            }
        }


        /// <summary>
        /// apply serialization process.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="skipRehydration">if set, don't bother rehydrating; assume obj has been filled with .Include as needed. Primarily for unit testing.</param>
        /// <returns>serialized string</returns>
        public string Serialize(TEntity obj, bool skipRehydration = false)
        {
            if (Mode != SerializationSessionMode.Serialization)
            {
                throw new InvalidOperationException("Mode isn't valid for Serialization");
            }

            if (!skipRehydration)  //skip true primarily for UNIT TESTING.
            {
                obj = _rehydrator.Rehydrate(obj);  //Hydrate it\\
            }

            var sb = new StringBuilder();
            var jwriter = new JsonTextWriter(new StringWriter(sb));
            _js.Serialize(jwriter, obj);
            return sb.ToString();
        }

        /// <summary>
        /// Deserialize
        /// After calling this, we are nearly ready to call .SaveChanges on the dbcontext to update it with any changes deserialized.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public TEntity Deserialize(string str)
        {
            if (Mode != SerializationSessionMode.Deserialization)
            {
                throw new InvalidOperationException("Mode isn't valid for Deserialization");
            }

            return _des.Deserialize(str);
        }
    }
}
