using System.ComponentModel.DataAnnotations;

namespace BookStore.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Tài khoản")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}