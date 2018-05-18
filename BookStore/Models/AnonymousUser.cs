using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class AnonymousUser : BaseEntity
    {
        [Display(Name = "Tên")]
        public string FirstName { get; set; }

        [Display(Name = "Họ")]
        public string LastName { get; set; }
        
        public DateTime? Birthday { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Số điện thoại")]

        public string PhoneNumber { get; set; }
        public AnonymousUser() { }
        public AnonymousUser(string firstname, string lastname, string email, string address, string phone) {
            FirstName = firstname;
            LastName = lastname;
            Email = email;
            Address = address;
            PhoneNumber = phone;
        }
    }
}