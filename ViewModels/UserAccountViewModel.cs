using System.ComponentModel.DataAnnotations;

namespace DoAn_web.ViewModels
{
    public class UserAccountViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(255)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái và số")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có từ 6 đến 100 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quyền")]
        [RegularExpression("^[AC]$", ErrorMessage = "Quyền tài khoản không hợp lệ")]
        public string UserRole { get; set; }
    }
}
