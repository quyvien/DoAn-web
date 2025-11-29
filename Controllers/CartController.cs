// File: /Controllers/CartController.cs
using DoAn_web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DoAn_web.Controllers
{
    public class CartController : Controller
    {
        private MyStore2026Entities db = new MyStore2026Entities();

        // Lấy giỏ hàng từ Session
        private List<CartItem> GetCart()
        {
            List<CartItem> cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // Lưu giỏ hàng vào Session
        private void SaveCart(List<CartItem> cart)
        {
            Session["Cart"] = cart;
        }

        // ===== HÀM DÙNG CHUNG: THÊM 1 SẢN PHẨM VÀO GIỎ =====
        // Trả về true nếu thêm OK, false nếu không đủ hàng / lỗi
        private bool AddItemToCart(int productID, int quantity)
        {
            List<CartItem> cart = GetCart();
            CartItem item = cart.Where(i => i != null)
                                .FirstOrDefault(i => i.ProductID == productID);

            var productDB = db.Products.Find(productID);

            // Tính số lượng sau khi cộng thêm
            int requiredQuantity = quantity;
            if (item != null)
            {
                requiredQuantity = item.Quantity + quantity;
            }

            // Kiểm tra tồn kho
            if (productDB == null || productDB.Quantity < requiredQuantity)
            {
                TempData["Error"] = $"Sản phẩm {productDB?.ProductName ?? "này"} chỉ còn {productDB?.Quantity ?? 0} sản phẩm. Vui lòng giảm số lượng.";
                return false;
            }
            ////// bắt đầu flassh sale 
            decimal finalPrice = productDB.ProductPrice;
            // tim coi sp co dang chay flash sale ko
            var activeFlashSale = db.FlashSaleItems.FirstOrDefault(f =>
                                   f.ProductID == productID &&
                                   f.IsActive == true &&
                                   f.StartDate <= DateTime.Now &&
                                   f.EndDate >= DateTime.Now);
            // kiem tra coi con suat giam gia ko
            if(activeFlashSale != null && activeFlashSale.SoldQuantity<activeFlashSale.SaleQuantityLimit)
            {
                finalPrice = activeFlashSale.SalePrice;
                // neu thoa man thi lay gia tri ale
            }

            // Nếu đã có trong giỏ → tăng số lượng
            if (item != null)
            {
                item.Quantity += quantity;
                // cap nhat gia moi
                item.UnitPrice = finalPrice;
                item.OriginalPrice = productDB.ProductPrice; // cap nhat gia moi nhat
            }
            else
            {
                // Nếu chưa có → tạo item mới
                var newItem = new CartItem
                {
                    ProductID = productDB.ProductID,
                    ProductName = productDB.ProductName,
                    ProductImage = productDB.ProductImage,
                    Quantity = quantity,
                    UnitPrice = finalPrice,
                    OriginalPrice = productDB.ProductPrice
                };
                cart.Add(newItem);
            }

            SaveCart(cart);
            return true;
        }
        // ================== HẾT HÀM DÙNG CHUNG ==================

        // GET: /Cart/Index
        public ActionResult Index()
        {
            List<CartItem> cart = GetCart();

            if (cart.Count == 0)
            {
                ViewBag.CartMessage = "Giỏ hàng của bạn đang trống.";
            }

            return View(cart);
        }

        // POST: /Cart/AddToCart  (thêm 1 sản phẩm)
        [HttpPost]
        public ActionResult AddToCart(int productID, int quantity)
        {
            bool ok = AddItemToCart(productID, quantity);
            // Nếu lỗi tồn kho thì TempData["Error"] đã có message
            return RedirectToAction("Index");
        }

        // POST: /Cart/AddComboToCart  (máy + 3 phụ kiện)
        [HttpPost]
        public ActionResult AddComboToCart(int productID, int[] accessoryIds)
        {
            // Thêm sản phẩm chính
            if (!AddItemToCart(productID, 1))
            {
                return RedirectToAction("Index");
            }

            // Thêm từng phụ kiện
            if (accessoryIds != null)
            {
                foreach (var accId in accessoryIds)
                {
                    if (!AddItemToCart(accId, 1))
                    {
                        // Nếu có phụ kiện thiếu hàng thì dừng lại
                        break;
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // GET: /Cart/RemoveFromCart/5
        public ActionResult RemoveFromCart(int productID)
        {
            List<CartItem> cart = GetCart();
            CartItem item = cart.Where(i => i != null)
                                .FirstOrDefault(i => i.ProductID == productID);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ChangeQuantity(int productID, int change)
        {
            List<CartItem> cart = GetCart();
            CartItem item = cart.Where(i => i != null)
                                .FirstOrDefault(i => i.ProductID == productID);

            if (item == null)
                return RedirectToAction("Index");

            var productDB = db.Products.Find(productID);
            if (productDB == null)
                return RedirectToAction("Index");

            int newQuantity = item.Quantity + change;

            if (newQuantity <= 0)
            {
                cart.Remove(item);
                SaveCart(cart);
                return RedirectToAction("Index");
            }

            if (newQuantity > productDB.Quantity)
            {
                TempData["Error"] = $"Sản phẩm {productDB.ProductName} chỉ còn {productDB.Quantity} sản phẩm. Vui lòng giảm số lượng.";
                return RedirectToAction("Index");
            }

            item.Quantity = newQuantity;
            SaveCart(cart);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult ChangeQuantityAjax(int productID, int change)
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
                return Json(new { success = false, message = "Giỏ hàng trống" },
                            JsonRequestBehavior.AllowGet);

            var item = cart.FirstOrDefault(x => x.ProductID == productID);
            if (item == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" },
                            JsonRequestBehavior.AllowGet);

            item.Quantity += change;
            if (item.Quantity < 1)
                item.Quantity = 1;

            decimal cartTotal = cart.Sum(x => x.TotalPrice);

            return Json(new
            {
                success = true,
                productID = productID,
                quantity = item.Quantity,
                itemTotal = item.TotalPrice,
                cartTotal = cartTotal
            }, JsonRequestBehavior.AllowGet);
        }



        // Dọn dẹp DbContext
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private int GetCurrentCustomerID()
        {
            var username = User.Identity.Name;
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer != null)
            {
                return customer.CustomerID;
            }
            return 0;
        }

        [Authorize(Roles = "C")]
        public ActionResult Checkout()
        {
            List<CartItem> cart = GetCart();
            if (cart.Count == 0 || cart.Any(i => i == null))
            {
                return RedirectToAction("Index");
            }

            int customerID = GetCurrentCustomerID();
            var customer = db.Customers.Find(customerID);

            ViewBag.Cart = cart;
            ViewBag.Customer = customer;
            // lay thong tin giam gia tu session neu co
            ViewBag.DiscountID = Session["DiscountID"];
            ViewBag.DiscountAmount = (decimal?)Session["DiscountAmount"] ?? 0 ;
            decimal subtotal = GetCartSubtotal(cart);
            decimal finalTotal = subtotal - (decimal)ViewBag.DiscountAmount;
            ViewBag.FinalTotal = finalTotal;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "C")]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string AddressDelivery, decimal TotalAmount, string PaymentMethod)
        {
            List<CartItem> cart = GetCart();
            if (cart.Count == 0 || cart.Any(i => i == null))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var order = new Order();
                order.CustomerID = GetCurrentCustomerID();
                order.OrderDate = DateTime.Now;
                order.TotalAmount = TotalAmount;
                order.AddressDelivery = AddressDelivery;

                order.PaymentStatus = (PaymentMethod == "Tiền mặt (COD)")
                                      ? "Chờ xử lý"
                                      : "Chờ xác nhận chuyển khoảng";

                order.ShippingStatus = "Chờ lấy hàng";
                // lay thong tin tu session da luu trong ApplyCoupon
                order.DiscountID = (int?)Session["DiscountID"];
                order.DiscountAmount = (decimal?)Session["DiscountAmount"] ?? 0; // Lưu số tiền giảm

                db.Orders.Add(order);
                db.SaveChanges(); // để có OrderID

                foreach (var item in cart)
                {
                    if (item != null)
                    {
                        var product = db.Products.Find(item.ProductID);

                        if (product != null && product.Quantity >= item.Quantity)
                        {
                            var detail = new OrderDetail();
                            detail.OrderID = order.OrderID;
                            detail.ProductID = item.ProductID;
                            detail.Quantity = item.Quantity;
                            detail.UnitPrice = item.UnitPrice;
                            db.OrderDetails.Add(detail);
                            // ton kho
                            product.Quantity = product.Quantity - item.Quantity;
                            product.SoldQuantity = product.SoldQuantity + item.Quantity;
                            // flash sale
                            var flashSaleItem = db.FlashSaleItems.FirstOrDefault(f =>
                         f.ProductID == item.ProductID &&
                         f.IsActive == true &&
                         f.StartDate <= DateTime.Now &&
                         f.EndDate >= DateTime.Now);
                            if (flashSaleItem != null)
                            {
                                // Cộng thêm số lượng khách vừa mua vào Flash Sale
                                flashSaleItem.SoldQuantity += item.Quantity;
                            }
                        }
                        else
                        {
                            throw new Exception($"Sản phẩm {item.ProductName} không đủ số lượng tồn kho (Hiện còn: {product?.Quantity ?? 0}).");
                        }
                    }
                }

                db.SaveChanges();
                // xoa session sau khi dat hang thanh cong
                Session["Cart"] = null;
                Session["DiscountID"] = null; // xoa discountid
                Session["DiscountAmount"] = null;// xoa discountAmount
                TempData["OrderSuccessMessage"] = "Đặt hàng thành công!";

                return RedirectToAction("OrderHistory", "Default");
            }
            catch (Exception ex)
            {
                ViewBag.DiscountAmount = (decimal?)Session["DiscountAmount"] ?? 0;
                ViewBag.FinalTotal = GetCartSubtotal(cart) - ViewBag.DiscountAmount;
                ViewBag.Error = "Lỗi đặt hàng: " + ex.Message;

                ViewBag.Cart = cart;
                ViewBag.Customer = db.Customers.Find(GetCurrentCustomerID());
                return View("Checkout");
            }
        }

        public ActionResult OrderConfirmation()
        {
            if (TempData["OrderSuccessMessage"] == null)
            {
                return RedirectToAction("Index", "Default");
            }
            return View();
        }
        // ham2= tinh tong tien gio hang
        private decimal GetCartSubtotal(List<CartItem> cart)
        {
            if (cart == null || cart.Count == 0 || cart.Any(i => i == null)) return 0;
            return cart.Sum(i => i.Quantity * i.UnitPrice);
        }
        // ham xu li giam gia neu co
        [HttpPost]
        public ActionResult ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                TempData["CouponError"] = "Vui lòng nhập mã giảm giá.";
                return RedirectToAction("Checkout");
            }
            List<CartItem> cart = GetCart();
            decimal cartSubtotal = GetCartSubtotal(cart);
            var discount = db.Discounts
                    .FirstOrDefault(d => d.CouponCode == couponCode &&
                    d.IsActive &&
                    d.StartDate <= DateTime.Now &&
                    d.EndDate >= DateTime.Now &&
                    (d.MinimumOrderAmount == null || cartSubtotal >= d.MinimumOrderAmount)); // Check min amount
            if (discount == null)
            {
                TempData["Error"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
                Session["DiscountAmount"] = null;
                Session["AppliedCoupon"] = null;
                return RedirectToAction("Checkout");
            }
            decimal discountAmount = 0;
            if (discount.DiscountType == "Percentage")// giam theo phan tram
            {
                discountAmount = cartSubtotal * (discount.DiscountValue / 100);
                // ap dung gioi han giam gia toi da  neu co
                if (discount.MaxDiscountAmount.HasValue && discountAmount > discount.MaxDiscountAmount.Value)
                {
                    discountAmount = discount.MaxDiscountAmount.Value;
                }
            }
            else if (discount.DiscountType == "FixedAmount")// 50.000d
            {
                discountAmount = discount.DiscountValue;
            }
            // ko giam tien
            discountAmount = Math.Min(discountAmount, cartSubtotal);
            // luu thong tin giam gia vao session de su dung khi dat hang
            Session["DiscountAmount"] = discountAmount;
            Session["DiscountID"]= discount.DiscountID;
            TempData["Success"] = $"Áp dụng mã {couponCode} thành công! Giảm: {discountAmount:N0} VNĐ. ";
            return RedirectToAction("Checkout");


        }


    }
}
