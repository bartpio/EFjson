using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EFjson.HashingBase
{
    /// <summary>
    /// Hasher of arbitrary objects, powered by Newtonsoft's JSON serialization (BSON variant is used)
    /// </summary>
    public class Hasher
    {
        /// <summary>
        /// crypto transformation for stream based hashing purposes.
        /// </summary>
        private readonly Func<HashAlgorithm> _ict;

        /// <summary>
        /// store a json serializer
        /// </summary>
        private readonly JsonSerializerSettings _jss;

        /// <summary>
        /// If desired, provide an HMAC key 
        /// </summary>
        /// <param name="hmacKey">Optional HMAC key, if desired to obscure the meaning of the hash tokens</param>
        public Hasher(JsonSerializerSettings jss, byte[] hmacKey = null)
        {
            jss = jss ?? new JsonSerializerSettings(); //Start with blanks if need be.

            if (hmacKey != null)
            {
                _ict = () => new HMACSHA256(hmacKey);
            }
            else
            {
                _ict = () => new SHA256Managed();
            }

            _jss = jss;
        }

        /// <summary>
        /// Cons, given an HMAC key as a string
        /// </summary>
        /// <param name="hmacKeyString">Optional HMAC key, if desired to obscure the meaning of the hash tokens</param>
        public Hasher(JsonSerializerSettings jss, string hmacKeyString) : this(jss, Encoding.Unicode.GetBytes(hmacKeyString))
        {
        }

        /// <summary>
        /// For sync.
        /// </summary>
        private readonly object _locker = new object();

        /// <summary>
        /// Go ahead and compute hash of the specified object
        /// </summary>
        /// <param name="o">Arbitrary obj that will be subject to JSON serialization (BSON variant) then hashing</param>
        /// <returns>Hash of the supplied object, as a base64 string</returns>
        public string ComputeHash(object o)
        {
            using (var ict = _ict())
            {
                using (var cs = new CryptoStream(Stream.Null, ict, CryptoStreamMode.Write))
                {
                    using (var writer = new BsonDataWriter(cs))
                    {
                        var js = JsonSerializer.Create(_jss);
                        js.Serialize(writer, o);
                    }

                    return Convert.ToBase64String(ict.Hash);
                }
            }
        }
    }
}
