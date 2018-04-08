using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class ProductItemView
    {
        private int id { get; set; }
        private int prdId { get; set; }

        public ProductItemView(int id, int prdId)
        {
            this.id = id;
            this.prdId = prdId;
        }
    }
}