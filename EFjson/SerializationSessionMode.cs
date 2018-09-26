using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson
{
    /// <summary>
    /// Are we serializing or deserializing?
    /// </summary>
    public enum SerializationSessionMode : byte
    {
        /// <summary>
        /// We are serializing
        /// </summary>
        Serialization,

        /// <summary>
        /// We are deserializing
        /// </summary>
        Deserialization
    }
}
