using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn_web.Models
{
    public class DiscountMetadata
    {
        [Display(Name = "Tên chương trình")]
        [Required(ErrorMessage = "Tên chương trình không được để trống.")]
        public string DiscountName { get; set; }
        [Display(Name = "Loại giảm giá")]
        [Required(ErrorMessage = "Loại giảm giá không được để trống.")]
        public string DiscountType { get; set; }
        [Display(Name = "Giá trị giảm giá")]
        [Required(ErrorMessage = "Giá trị giảm giá không được để trống.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0.")]
        [DataType(DataType.Currency)]
        public decimal DiscountValue { get; set; }
        [Display(Name = "Mã coupon")]
        [StringLength(50,ErrorMessage = "Mã Coupon không được vượt quá 50 ký tự.")]
        // thhem  Remote de kiem tra ma coupon da ton tai chua
        [Remote("CheckCouponCodeExists", "Discount", AdditionalFields = "DiscountID", ErrorMessage = "Mã Coupon đã tồn tại.")]
        public string CouponCode { get; set; }
        [Display(Name = "Ngày bắt đầu")]
        [Required(ErrorMessage = "Ngày bắt đầu không được để trống.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public System.DateTime StartDate { get; set; }
        [Display(Name = "Ngày kết thúc")]
        [Required(ErrorMessage = "Ngày kết thúc không được để trống.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public System.DateTime EndDate { get; set; }
        [Display(Name = "Đơn hàng tối thiểu (VNĐ)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá trị phải hợp lệ.")]
        public Nullable<decimal> MinimumOrderAmount { get; set; }
        [Display(Name = "Mức giảm tối đa (VNĐ)")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá trị phải hợp lệ.")]
        public Nullable<decimal> MaxDiscountAmount { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; }

    }
}