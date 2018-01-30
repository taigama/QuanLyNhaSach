namespace BookStore.Models
{
    public class OrderDetail : BaseEntity
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public double TotalAmount { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }
    }
}