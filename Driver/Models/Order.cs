using System.Collections.Generic;

namespace Driver.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public double TotalPrice { get; set; }
        public List<Product> Products { get; set; }
    }
}
