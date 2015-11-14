using System.ComponentModel.DataAnnotations;

namespace Csf.FileUpload.Models
{
    public class User
    {

        public int UserId { get; set; }
        [Required, Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required, Display(Name = "密码")]
        [DataType(DataType.Password)]
        public string UserPwd { get; set; }

        [Required, Display(Name = "电话")]
        [DataType(DataType.PhoneNumber)]
        public string Tel { get; set; }

        [Required, Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "头像")]
        public string Avatar { get; set; }
    }
}
