using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class ProductDetail : BaseEntity
    {
        public int ProductId { get; set; }

        public int? AuthorId { get; set; }

        public virtual Author Author { get; set; }

        public virtual Product Product { get; set; }
    }
}