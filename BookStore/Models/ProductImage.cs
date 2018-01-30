using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class ProductImage : BaseEntity
    {
        public int? ProductId { get; set; }

        public virtual Product Product { get; set; }

        public string Url { get; set; }
    }
}