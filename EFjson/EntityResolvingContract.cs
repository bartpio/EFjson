using EFjson.Interfaces;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson
{
    /// <summary>
    /// abstract base for entity-resolving contract
    /// </summary>
    /// <typeparam name="THashKey"></typeparam>
    public class EntityResolvingContract<THashKey> : JsonObjectContract, IHasDbContext where THashKey : class
    {
        /// <summary>
        /// entity key hasher
        /// </summary>
        private readonly IEntityKeyHasher<THashKey> _iekh;

        /// <summary>
        /// entity key unhasher
        /// </summary>
        private readonly IEntityKeyUnhasher<THashKey> _unhasher;

        /// <summary>
        /// access to mode
        /// </summary>
        public SerializationSessionMode Mode => _unhasher != null ? SerializationSessionMode.Deserialization : SerializationSessionMode.Serialization;

        /// <summary>
        /// cons, given underlying
        /// </summary>
        /// <param name="underlyingType">type of a particular entity</param>
        /// <param name="dbc">db context we are working with</param>
        /// <param name="uhmap">provide a set here, which can be empty. Will be used to track unhasher resolutions.</param>
        /// <param name="iekh">for serialization operations, provide hasher to hash entity keys</param>
        /// <param name="unhasher">unhasher required for DESERIALIZATION only.</param>
        public EntityResolvingContract(Type underlyingType, DbContext dbc, ISet<EntityKey> keyset, IEntityKeyHasher<THashKey> iekh, IEntityKeyUnhasher<THashKey> unhasher = null) : base(underlyingType)
        {
            MyDbContext = dbc;
            _iekh = iekh;
            _unhasher = unhasher;

            //setup creator parameters
            CreatorParameters.Add(new JsonProperty() { PropertyName = "_ek", PropertyType = typeof(THashKey) });

            //local alias for conv
            var oc = this.GetObjectContext();

            //set override creator
            OverrideCreator = (objectz) =>
            {
                if (Mode != SerializationSessionMode.Deserialization)
                {
                    throw new InvalidOperationException("Can't configure are we are not set for SerializationSessionMode.Deserialization");
                }

                var thestring = objectz[0] as THashKey;

                if (thestring != null)
                {
                    var thek = _unhasher.Unhash(thestring);
                    keyset?.Add(thek);

                    if (oc.TryGetObjectByKey(thek, out var preobj))
                    {
                        return preobj;
                    }

                }

                return Activator.CreateInstance(underlyingType);
            };

            //During serialization, also incl _ek token slot
            ExtensionDataGetter = o =>
            {
                var ek0 = oc.ObjectStateManager.GetObjectStateEntry(o).EntityKey;
                return new Dictionary<object, object> { ["_ek"] = _iekh.ComputeHash(ek0) };
            };
        }

        /// <summary>
        /// access to my db context
        /// </summary>
        public DbContext MyDbContext { get; }
    }
}
