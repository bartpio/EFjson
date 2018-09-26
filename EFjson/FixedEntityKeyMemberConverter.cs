using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson
{
    /// <summary>
    /// This subclass provides a fixed version of Newtonsoft's EntityKeyMemberConverter
    /// The original class provided (as of newtonsoft v10) tests for an outdated EntityKeyMember type
    /// </summary>
    public sealed class FixedEntityKeyMemberConverter : EntityKeyMemberConverter
    {
        /// <summary>
        /// Can convert? Here we test the proper EntityKeyMember type in modern Entity Framework
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(EntityKeyMember).IsAssignableFrom(objectType);
        }
    }
}
