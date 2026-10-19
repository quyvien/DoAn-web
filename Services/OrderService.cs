using DoAn_web.Models;
using System;
using System.Linq;

namespace DoAn_web.Services
{
    public static class OrderService
    {
        public const string CancelledStatus = "Đã hủy";

        public static bool IsCancelled(Order order)
        {
            return order != null && (IsCancelledStatus(order.PaymentStatus) || IsCancelledStatus(order.ShippingStatus));
        }

        public static bool CanCustomerCancel(Order order)
        {
            if (order == null || IsCancelled(order))
            {
                return false;
            }

            var canCancelPayment = order.PaymentStatus == "Chờ xác nhận chuyển khoản" ||
                                   order.PaymentStatus == "Chờ xác nhận chuyển khoảng" ||
                                   order.PaymentStatus == "Chờ xử lý" ||
                                   order.PaymentStatus == "Chưa thanh toán";

            return canCancelPayment && order.ShippingStatus == "Chờ lấy hàng";
        }

        public static bool CancelOrder(MyStore2026Entities db, Order order)
        {
            if (order == null)
            {
                return false;
            }

            // Dữ liệu cũ có nhiều cách viết trạng thái hủy; chỉ chuẩn hóa, không hoàn kho lần hai.
            if (IsCancelled(order))
            {
                order.PaymentStatus = CancelledStatus;
                order.ShippingStatus = CancelledStatus;
                return true;
            }

            if (!CanCustomerCancel(order))
            {
                throw new InvalidOperationException("Đơn hàng không còn có thể hủy.");
            }

            foreach (var detail in order.OrderDetails)
            {
                var product = db.Products.Find(detail.ProductID);
                if (product != null)
                {
                    product.Quantity += detail.Quantity;
                    product.SoldQuantity = Math.Max(0, product.SoldQuantity - detail.Quantity);
                }

                var flashSale = db.FlashSaleItems.FirstOrDefault(f =>
                    f.ProductID == detail.ProductID &&
                    f.StartDate <= order.OrderDate &&
                    f.EndDate >= order.OrderDate &&
                    f.SalePrice == detail.UnitPrice);

                if (flashSale != null)
                {
                    flashSale.SoldQuantity = Math.Max(0, flashSale.SoldQuantity - detail.Quantity);
                }
            }

            order.PaymentStatus = CancelledStatus;
            order.ShippingStatus = CancelledStatus;
            return true;
        }

        private static bool IsCancelledStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            var normalized = status.Trim().ToLowerInvariant();
            return normalized.Contains("hủy") || normalized.Contains("huỷ");
        }
    }
}
