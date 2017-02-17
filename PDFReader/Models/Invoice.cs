using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReader.Models
{
    public class Invoice
    {
        public int Ordered { get; set; }
        public string ItemNumber { get; set; }
        public string Description { get; set; }
        public int Shipped { get; set; }
        public double UnitPrice { get; set; }
        public string UnitOfMeasure { get; set; }
        public double Amount { get; set; }
    }
}
