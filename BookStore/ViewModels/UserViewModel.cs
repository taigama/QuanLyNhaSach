using System.ComponentModel.DataAnnotations;
using System;

namespace BookStore.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Tên")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập Họ")]
        [Display(Name = "Họ")]
        public string LastName { get; set; }

        [ScaffoldColumn(false)]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Địa chỉ")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(12, ErrorMessage = "Số điện thoại phải có từ 8 đến 12 số.", MinimumLength = 8)]
        public string PhoneNumber { get; set; }
    }
}