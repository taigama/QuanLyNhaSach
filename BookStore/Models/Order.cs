using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Order : BaseEntity
    {
        [Display(Name ="Trạng thái")]
        public OrderStatus Status { get; set; }

        [Display(Name = "Ngày tạo")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày chuyển tới")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        public int? UserId { get; set; }

        public int? AnonymousUserId { get; set; }

        [Display(Name = "Tổng số tiền")]
        public double TotalAmount { get; set; }

        [Display(Name = "Người lập")]
        public virtual User User { get; set; }

        [Display(Name = "Khách hàng")]
        public virtual AnonymousUser AnonymousUser { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}