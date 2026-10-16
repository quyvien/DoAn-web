// File: /Controllers/CartController.cs
using DoAn_web.Models;
using System;
using System.Collections.Generic;
using System.Data;
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
            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng sản phẩm phải lớn hơn 0.";
                return false;
            }

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
            if (activeFlashSale != null && activeFlashSale.SoldQuantity + requiredQuantity <= activeFlashSale.SaleQuantityLimit)
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
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productID, int quantity)
        {
            bool ok = AddItemToCart(productID, quantity);
            // Nếu lỗi tồn kho thì TempData["Error"] đã có message
            return RedirectToAction("Index");
        }

        // POST: /Cart/AddComboToCart  (máy + 3 phụ kiện)
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
            RefreshCartPrices(cart, DateTime.Now);

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            var product = db.Products.Find(productID);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không còn tồn tại" });

            var newQuantity = Math.Max(1, item.Quantity + change);
            if (newQuantity > product.Quantity)
                return Json(new { success = false, message = $"Sản phẩm chỉ còn {product.Quantity} sản phẩm" });

            item.Quantity = newQuantity;
            RefreshCartPrices(cart, DateTime.Now);

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

            try
            {
                RefreshCartPrices(cart, DateTime.Now);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }

            int customerID = GetCurrentCustomerID();
            var customer = db.Customers.Find(customerID);
            if (customer == null)
            {
                return new HttpStatusCodeResult(403);
            }

            ViewBag.Cart = cart;
            ViewBag.Customer = customer;
            // lay thong tin giam gia tu session neu co
            decimal subtotal = GetCartSubtotal(cart);
            int? discountID;
            decimal discountAmount = GetCurrentDiscountAmount(subtotal, DateTime.Now, out discountID);
            Session["DiscountID"] = discountID;
            Session["DiscountAmount"] = discountAmount;
            ViewBag.DiscountID = discountID;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.FinalTotal = subtotal - discountAmount;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "C")]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string AddressDelivery, string PaymentMethod)
        {
            List<CartItem> cart = GetCart();
            if (cart.Count == 0 || cart.Any(i => i == null))
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(AddressDelivery) || AddressDelivery.Trim().Length > 500)
            {
                return ShowCheckoutError(cart, "Vui lòng nhập địa chỉ giao hàng hợp lệ.");
            }

            if (PaymentMethod != "Tiền mặt (COD)" && PaymentMethod != "Chuyển khoản")
            {
                return ShowCheckoutError(cart, "Phương thức thanh toán không hợp lệ.");
            }

            try
            {
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var now = DateTime.Now;
                    RefreshCartPrices(cart, now);

                    var customerID = GetCurrentCustomerID();
                    if (customerID == 0)
                    {
                        throw new InvalidOperationException("Không tìm thấy thông tin khách hàng.");
                    }

                    var subtotal = GetCartSubtotal(cart);
                    int? discountID;
                    var discountAmount = GetCurrentDiscountAmount(subtotal, now, out discountID);
                    var order = new Order
                    {
                        CustomerID = customerID,
                        OrderDate = now,
                        TotalAmount = subtotal - discountAmount,
                        AddressDelivery = AddressDelivery.Trim(),
                        PaymentStatus = PaymentMethod == "Tiền mặt (COD)" ? "Chờ xử lý" : "Chờ xác nhận chuyển khoản",
                        ShippingStatus = "Chờ lấy hàng",
                        DiscountID = discountID,
                        DiscountAmount = discountAmount
                    };

                    foreach (var item in cart)
                    {
                        var product = db.Products.Find(item.ProductID);
                        if (product == null || product.Quantity < item.Quantity)
                        {
                            throw new InvalidOperationException($"Sản phẩm {item.ProductName} không đủ số lượng tồn kho.");
                        }

                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });
                        product.Quantity -= item.Quantity;
                        product.SoldQuantity += item.Quantity;

                        var flashSaleItem = FindAvailableFlashSale(item.ProductID, item.Quantity, now);
                        if (flashSaleItem != null && item.UnitPrice == flashSaleItem.SalePrice)
                        {
                            flashSaleItem.SoldQuantity += item.Quantity;
                        }
                    }

                    db.Orders.Add(order);
                    db.SaveChanges();
                    transaction.Commit();
                }

                // xoa session sau khi dat hang thanh cong
                Session["Cart"] = null;
                Session["DiscountID"] = null; // xoa discountid
                Session["DiscountAmount"] = null;// xoa discountAmount
                TempData["OrderSuccessMessage"] = "Đặt hàng thành công!";

                return RedirectToAction("OrderHistory", "Default");
            }
            catch (InvalidOperationException ex)
            {
                return ShowCheckoutError(cart, ex.Message);
            }
            catch (Exception)
            {
                return ShowCheckoutError(cart, "Không thể tạo đơn hàng lúc này. Vui lòng thử lại sau.");
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

        private FlashSaleItem FindAvailableFlashSale(int productID, int quantity, DateTime now)
        {
            return db.FlashSaleItems.FirstOrDefault(f =>
                f.ProductID == productID &&
                f.IsActive &&
                f.StartDate <= now &&
                f.EndDate >= now &&
                f.SoldQuantity + quantity <= f.SaleQuantityLimit);
        }

        private void RefreshCartPrices(List<CartItem> cart, DateTime now)
        {
            foreach (var item in cart)
            {
                if (item == null || item.Quantity <= 0)
                {
                    throw new InvalidOperationException("Giỏ hàng chứa sản phẩm không hợp lệ.");
                }

                var product = db.Products.Find(item.ProductID);
                if (product == null)
                {
                    throw new InvalidOperationException("Một sản phẩm trong giỏ không còn tồn tại.");
                }

                if (product.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Sản phẩm {product.ProductName} chỉ còn {product.Quantity} sản phẩm.");
                }

                var flashSale = FindAvailableFlashSale(product.ProductID, item.Quantity, now);
                item.ProductName = product.ProductName;
                item.ProductImage = product.ProductImage;
                item.OriginalPrice = product.ProductPrice;
                item.UnitPrice = flashSale == null ? product.ProductPrice : flashSale.SalePrice;
            }

            SaveCart(cart);
        }

        private decimal GetCurrentDiscountAmount(decimal subtotal, DateTime now, out int? discountID)
        {
            discountID = Session["DiscountID"] as int?;
            if (!discountID.HasValue)
            {
                return 0;
            }

            var discount = db.Discounts.Find(discountID.Value);
            if (discount == null || !discount.IsActive || discount.StartDate > now || discount.EndDate < now ||
                (discount.MinimumOrderAmount.HasValue && subtotal < discount.MinimumOrderAmount.Value))
            {
                discountID = null;
                return 0;
            }

            decimal amount;
            if (discount.DiscountType == "Percentage")
            {
                amount = subtotal * discount.DiscountValue / 100;
                if (discount.MaxDiscountAmount.HasValue)
                {
                    amount = Math.Min(amount, discount.MaxDiscountAmount.Value);
                }
            }
            else if (discount.DiscountType == "FixedAmount")
            {
                amount = discount.DiscountValue;
            }
            else
            {
                discountID = null;
                return 0;
            }

            return Math.Max(0, Math.Min(amount, subtotal));
        }

        private ActionResult ShowCheckoutError(List<CartItem> cart, string message)
        {
            var subtotal = GetCartSubtotal(cart);
            int? discountID;
            var discountAmount = GetCurrentDiscountAmount(subtotal, DateTime.Now, out discountID);
            ViewBag.Cart = cart;
            ViewBag.Customer = db.Customers.Find(GetCurrentCustomerID());
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.FinalTotal = subtotal - discountAmount;
            ViewBag.Error = message;
            return View("Checkout");
        }

        // ham xu li giam gia neu co
        [HttpPost]
        [Authorize(Roles = "C")]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                TempData["CouponError"] = "Vui lòng nhập mã giảm giá.";
                return RedirectToAction("Checkout");
            }
            List<CartItem> cart = GetCart();
            try
            {
                RefreshCartPrices(cart, DateTime.Now);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
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
                Session["DiscountID"] = null;
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
