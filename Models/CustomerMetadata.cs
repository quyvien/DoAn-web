using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DoAn_web.Models
{
    public class CustomerMetadata
    {
        // Chúng ta không cần CustomerID ở đây vì nó là khóa chính

        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Display(Name = "Số điện thoại")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 chữ số")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Số điện thoại chỉ được chứa chữ số")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }

        // Bạn không cần đặt quy tắc cho Username ở đây
        // trừ khi bạn cũng muốn validate
        // 
        //
        [Required(ErrorMessage = "Tài khoản không được để trống")]
        [Display(Name = "Tên tài khoản")] // <-- THÊM DÒNG NÀY
        public string Username { get; set; }

    }
    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer
    {
        // Giữ trống phần này
    }
}