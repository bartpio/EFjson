using EFjson.Interfaces;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson
{
    /// <summary>
    /// entity contract resolver
    /// </summary>
    /// <typeparam name="THashKey"></typeparam>
    public class EntityContractResolver<THashKey> : DefaultContractResolver, IHasDbContext where THashKey : class
    {
        /// <summary>
        /// db context
        /// </summary>
        public DbContext MyDbContext { get; }

        /// <summary>
        /// entity key hasher
        /// </summary>
        private readonly IEntityKeyHasher<THashKey> _iekh;

        /// <summary>
        /// entity key unhasher
        /// </summary>
        private readonly IEntityKeyUnhasherWithKeyset<THashKey> _unhasher;

        /// <summary>
        /// A set of keys.
        /// </summary>
        private ISet<EntityKey> Keyset => _unhasher?.Keyset;

        /// <summary>
        /// applicable types
        /// </summary>
        private readonly ImmutableHashSet<Type> _applicables;

        /// <summary>
        /// cons the contract resolver
        /// </summary>
        /// <param name="dbc"></param>
        /// <param name="iekh"></param>
        /// <param name="keyset"></param>
        /// <param name="unhasher">This we need for DESER only.</param>
        public EntityContractResolver(DbContext dbc, IEntityKeyHasher<THashKey> iekh, IEntityKeyUnhasherWithKeyset<THashKey> unhasher = null)
        {
            MyDbContext = dbc;
            _iekh = iekh;
            _unhasher = unhasher;
            _applicables = GetAllTypes(dbc.GetType()).ToImmutableHashSet();
        }

        /// <summary>
        /// pull all possible entity types
        /// </summary>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        protected virtual IEnumerable<Type> GetAllTypes(Type dbContextType)
        {
            var types = dbContextType
                .GetProperties()
                .Where(prop => prop.PropertyType.IsGenericType)
                .Where(prop => prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(prop => prop.PropertyType.GenericTypeArguments.First())
                .Distinct();

            return types;
        }

        /// <summary>
        /// create obj contract
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            if (_applicables.Contains(objectType))
            {
                var throwaway = base.CreateObjectContract(objectType);
                var manu = new EntityResolvingContract<THashKey>(objectType, MyDbContext, Keyset, _iekh, _unhasher)
                {
                    MemberSerialization = throwaway.MemberSerialization
                };

                //copy throwaway json props to real ones
                foreach (var item in throwaway.Properties)
                {
                    manu.Properties.Add(item);
                }

                return manu;
            }
            else
            {
                //for non entity cases let's just use a normal contract
                return base.CreateObjectContract(objectType);
            }
        }
    }
}
