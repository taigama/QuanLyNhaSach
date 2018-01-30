using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class ImportDetail : BaseEntity
    {
        public int ProductImportId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public double ImportPrice { get; set; }

        // Ngày nhập
        public DateTime ImportDate { get; set; }

        public virtual ProductImport Order { get; set; }

        public virtual Product Product { get; set; }
    }
}