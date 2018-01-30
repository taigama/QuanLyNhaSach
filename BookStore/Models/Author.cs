using System.Collections.Generic;

namespace BookStore.Models
{
    public class Author : BaseEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public int NumberOfBooks { get; set; }

        public virtual ICollection<ProductDetail> Books { get; set; }
    }
}