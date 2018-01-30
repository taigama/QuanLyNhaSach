using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookStore.Models
{
    public class User : BaseEntity
    {
        [Display(Name = "Tên nhân viên")]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime Birthday { get; set; }

        [Display(Name = "Tài khoản")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string Avatar { get; set; }

        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
    }
}