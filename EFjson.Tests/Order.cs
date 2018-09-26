using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFjson.Tests
{
    public class Order
    {
        /// <summary>
        /// cons.
        /// </summary>
        public Order()
        {
            Details = new HashSet<OrderDetail>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        public string Comment { get; set; }

        public ICollection<OrderDetail> Details { get; set; }
    }
}
