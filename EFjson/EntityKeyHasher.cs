using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFjson.HashingBase;
using EFjson.Interfaces;

namespace EFjson
{
    /// <summary>
    /// typed Entity Key hasher
    /// </summary>
    public sealed class EntityKeyHasher : Hasher, IEntityKeyHasher<string>
    {
        /// <summary>
        /// cons, given optional hmac key as buf
        /// </summary>
        /// <param name="jss"></param>
        /// <param name="hmacKey"></param>
        public EntityKeyHasher(byte[] hmacKey = null) : base(GetHasherSerializerSettings(), hmacKey)
        {
        }

        /// <summary>
        /// cons, given hmac key as str
        /// </summary>
        /// <param name="jss"></param>
        /// <param name="hmacKeyString"></param>
        public EntityKeyHasher(string hmacKeyString) : base(GetHasherSerializerSettings(), hmacKeyString)
        {
        }

        /// <summary>
        /// tweak serializer settings such that they're good for Entity Key serialization
        /// </summary>
        /// <param name="jss"></param>
        private static JsonSerializerSettings GetHasherSerializerSettings()
        {
            var jss = new JsonSerializerSettings();
            jss.Converters.Add(new FixedEntityKeyMemberConverter());
            return jss;
        }

        /// <summary>
        /// Typed compute hash
        /// </summary>
        /// <param name="ek"></param>
        /// <returns></returns>
        public string ComputeHash(EntityKey ek)
        {
            return ComputeHash((object)ek);
        }
    }
}
