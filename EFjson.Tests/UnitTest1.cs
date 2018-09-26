using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data.Entity;
using System.Linq;
using Newtonsoft.Json;

namespace EFjson.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestInitialize]
        public void Set()
        {
            using (var dc = new TestDataModel())
            {
                var someorder = new Order
                {
                    Comment = "some order"
                };

                dc.Orders.Add(someorder);

                for (var idx = 0; idx < 10; idx++)
                {
                    var det = new OrderDetail()
                    {
                        DetailComment = $"some detail {idx}"
                    };

                    someorder.Details.Add(det);
                }

                dc.SaveChanges();
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var dc = new TestDataModel())
            {
                var first = dc.Orders.FirstOrDefault();
                Assert.IsNotNull(first);
            }
        }

        [TestMethod]
        public void TestSerializer()
        {
            //var ekh = new EntityKeyHasher();

            

            using (var dc = new TestDataModel())
            {
                var first = dc.Orders.Include(x => x.Details).FirstOrDefault();
                Assert.IsNotNull(first);

              
                var serse = new OrderSerializationSession(dc, SerializationSessionMode.Serialization);
                var str = serse.Serialize(first);
                Assert.IsTrue(str.Contains("_ek"));
            }
        }

        [TestMethod]
        public void TestAcrossWireSimulation()
        {
            var str = String.Empty;
            var ekh = new EntityKeyHasher();

            using (var dc = new TestDataModel())
            {
                var first = dc.Orders.Where(x => x.Comment == "some order")  //NOTE... Here we don't bother with any .include!
                    .OrderByDescending(x => x.OrderId)
                    .FirstOrDefault();

                var serse = new OrderSerializationSession(dc, SerializationSessionMode.Serialization);
                str = serse.Serialize(first);
                Assert.IsTrue(str.Contains("_ek"));
            }

            //Here we simulate crossing the wire\\

            using (var dc2 = new TestDataModel())
            {
                var serse = new OrderSerializationSession(dc2, SerializationSessionMode.Deserialization);
                var deserred = serse.Deserialize(str);
                Assert.IsNotNull(deserred);
                Assert.IsTrue(deserred.Details.Count > 0);

                var deletenothing = new EntityDeleter(dc2, serse.Keyset);
                var deleted = deletenothing.MarkDeletedEntities();
                Assert.IsTrue(deleted.Count < 1, "nothing should have been deleted here");
            }
        }

        [TestMethod]
        public void TestAcrossWireSimulationWithdelete()
        {
            var str = String.Empty;
            var ekh = new EntityKeyHasher();

            using (var dc = new TestDataModel())
            {
                var first = dc.Orders.Where(x => x.Comment == "some order")  //NOTE... Here we don't bother with any .include!
                    .OrderByDescending(x => x.OrderId)
                    .FirstOrDefault();

               

                var serse = new OrderSerializationSession(dc, SerializationSessionMode.Serialization);
                str = serse.Serialize(first);

                //now that things have rehydrated let's delete and reser
                //Delete some things\\
                first.Details.Remove(first.Details.First());
                //Mod some things\\
                first.Details.Last().DetailComment = "This comment was modified.";

                str = serse.Serialize(first, true);  //this time we skip rehydration.
            }

            //Here we simulate crossing the wire\\

            using (var dc2 = new TestDataModel())
            {
                var serse = new OrderSerializationSession(dc2, SerializationSessionMode.Deserialization);
                var deserred = serse.Deserialize(str);
                Assert.IsNotNull(deserred);
                Assert.IsTrue(deserred.Details.Count > 0);

                var deletenothing = new EntityDeleter(dc2, serse.Keyset);
                var deleted = deletenothing.MarkDeletedEntities();
                Assert.IsTrue(deleted.Count == 1, "expecting to delete one entry exactly");
            }
        }
    }
}
