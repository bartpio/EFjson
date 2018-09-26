using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Newtonsoft.Json;

using EFjson.Interfaces;

namespace EFjson.Tests
{
    public class OrderSerializationSession : SerializationSession<Order, string>
    {
        /// <summary>
        /// cons
        /// </summary>
        /// <param name="dbc"></param>
        /// <param name="iekh"></param>
        /// <param name="jss"></param>
        public OrderSerializationSession(TestDataModel dbc, SerializationSessionMode mode) : base(dbc, new EntityKeyHasher(), new Rehydrator(dbc), mode, (new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        }))
        {
        }

        /// <summary>
        /// our very own rehydrator
        /// </summary>
        internal class Rehydrator : IEntityRehydrator<Order>
        {
            public TestDataModel _dc { get;  }

            public Rehydrator(TestDataModel dc)
            {
                _dc = dc;
            }

            public Order Rehydrate(Order dehydrated)
            {
                var dc = _dc;
                return dc.Orders
                    .Include(x => x.Details)
                    .Where(x => x.OrderId == dehydrated.OrderId)
                    .FirstOrDefault();
            }
        }
    }
}
