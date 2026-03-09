using AeroDroxUAV.Models;
using AeroDroxUAV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace AeroDroxUAV.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            IUserService userService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userService = userService;
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private int? GetCurrentUserId() => HttpContext.Session.GetInt32("UserId");

        // Checkout Page
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _cartService.GetUserCartAsync(userId.Value);
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items to proceed.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            var cartTotal = await _cartService.GetCartTotalAsync(userId.Value);
            var itemCount = await _cartService.GetCartItemCountAsync(userId.Value);

            var paymentModel = new PaymentViewModel
            {
                UserId = userId.Value,
                TotalAmount = cartTotal,
                ItemCount = itemCount,
                ShippingAddress = user?.Address ?? "",
                ShippingCity = user?.City ?? "",
                ShippingState = user?.State ?? "",
                ShippingPinCode = user?.PinCode ?? "",
                ShippingPhone = user?.MobileNumber ?? ""
            };

            ViewBag.CartItems = cartItems;
            return View(paymentModel);
        }

        // Process Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            try
            {
                if (!IsLoggedIn())
                {
                    TempData["Error"] = "Please login to continue.";
                    return RedirectToAction("Login", "Account");
                }

                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    HttpContext.Session.Clear();
                    TempData["Error"] = "Session expired. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine($"Processing payment for user {userId}");
                Console.WriteLine($"Payment Method: {model.PaymentMethod}");

                // Comprehensive validation
                var errors = new List<string>();

                // Validate shipping information
                if (string.IsNullOrWhiteSpace(model.ShippingAddress))
                    errors.Add("Shipping address is required");
                
                if (string.IsNullOrWhiteSpace(model.ShippingCity))
                    errors.Add("City is required");
                
                if (string.IsNullOrWhiteSpace(model.ShippingState))
                    errors.Add("State is required");
                
                if (string.IsNullOrWhiteSpace(model.ShippingPinCode))
                {
                    errors.Add("Pin code is required");
                }
                else if (!Regex.IsMatch(model.ShippingPinCode, @"^[1-9][0-9]{5}$"))
                {
                    errors.Add("Please enter a valid 6-digit pin code");
                }
                
                if (string.IsNullOrWhiteSpace(model.ShippingPhone))
                {
                    errors.Add("Phone number is required");
                }
                else if (!Regex.IsMatch(model.ShippingPhone, @"^[6-9][0-9]{9}$"))
                {
                    errors.Add("Please enter a valid 10-digit mobile number starting with 6-9");
                }

                // Validate payment method
                if (string.IsNullOrWhiteSpace(model.PaymentMethod))
                {
                    errors.Add("Please select a payment method");
                }

                // If there are validation errors, return to the form
                if (errors.Any())
                {
                    TempData["Error"] = string.Join("<br/>", errors);
                    var cartItems = await _cartService.GetUserCartAsync(userId.Value);
                    ViewBag.CartItems = cartItems;
                    return View("Index", model);
                }

                // Get cart items
                var cartItemsList = await _cartService.GetUserCartAsync(userId.Value);
                if (!cartItemsList.Any())
                {
                    TempData["Error"] = "Your cart is empty. Please add items to proceed.";
                    return RedirectToAction("Index", "Cart");
                }

                Console.WriteLine($"Found {cartItemsList.Count()} items in cart");

                // Process the order
                var success = await _orderService.CreateOrderAsync(userId.Value, model, cartItemsList.ToList());

                if (success)
                {
                    Console.WriteLine("Order created successfully");
                    
                    // Get the latest order for confirmation page
                    var orders = await _orderService.GetUserOrdersAsync(userId.Value);
                    Console.WriteLine($"Found {orders.Count()} orders for user after creation");
                    
                    var latestOrder = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault();
                    
                    if (latestOrder != null)
                    {
                        Console.WriteLine($"Redirecting to OrderConfirmation with ID: {latestOrder.Id}");
                        TempData["Success"] = "Your order has been placed successfully!";
                        return RedirectToAction("OrderConfirmation", new { id = latestOrder.Id });
                    }
                    
                    TempData["Success"] = "Your order has been placed successfully!";
                    return RedirectToAction("OrderHistory");
                }
                else
                {
                    Console.WriteLine("Failed to create order");
                    TempData["Error"] = "Failed to place order. Please try again.";
                    var cartItems2 = await _cartService.GetUserCartAsync(userId.Value);
                    ViewBag.CartItems = cartItems2;
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessPayment: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                TempData["Error"] = "An error occurred while processing your order: " + ex.Message;
                
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    var cartItems3 = await _cartService.GetUserCartAsync(userId.Value);
                    ViewBag.CartItems = cartItems3;
                }
                
                return View("Index", model);
            }
        }

        // Order Confirmation
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderWithDetailsAsync(id);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound();
            }

            return View(order);
        }

        // Order History
        [HttpGet]
        public async Task<IActionResult> OrderHistory()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetUserOrdersAsync(userId.Value);
            
            // Get additional stats for the view
            ViewBag.TotalOrders = orders.Count();
            ViewBag.TotalSpent = orders.Where(o => o.OrderStatus != "Cancelled").Sum(o => o.TotalAmount);
            ViewBag.PendingOrders = orders.Count(o => o.OrderStatus == "Pending" || o.OrderStatus == "Confirmed");
            ViewBag.DeliveredOrders = orders.Count(o => o.OrderStatus == "Delivered");
            
            return View(orders);
        }

        // Order Details
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderWithDetailsAsync(id);
            if (order == null || order.UserId != userId.Value)
            {
                return NotFound();
            }

            return View(order);
        }

        // Cancel Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var success = await _orderService.CancelOrderAsync(id, userId.Value);
            if (success)
            {
                TempData["Success"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Unable to cancel order. Only pending or confirmed orders can be cancelled.";
            }

            return RedirectToAction("OrderHistory");
        }

        // ========== ADMIN ORDER MANAGEMENT ==========

        [HttpGet]
        public async Task<IActionResult> AdminOrders(string status = "All")
        {
            if (!IsLoggedIn() || !IsAdmin())
                return Unauthorized();

            var orders = await _orderService.GetAllOrdersAsync();
            
            if (status != "All")
            {
                orders = orders.Where(o => o.OrderStatus == status);
            }

            ViewBag.CurrentStatus = status;
            ViewBag.OrderStats = await _orderService.GetOrderStatusStatsAsync();
            ViewBag.TotalRevenue = await _orderService.GetTotalRevenueAsync();
            ViewBag.TotalOrders = await _orderService.GetOrderCountAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> AdminOrderDetails(int id)
        {
            if (!IsLoggedIn() || !IsAdmin())
                return Unauthorized();

            var order = await _orderService.GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            if (!IsLoggedIn() || !IsAdmin())
                return Unauthorized();

            var success = await _orderService.UpdateOrderStatusAsync(id, orderStatus, paymentStatus);
            if (success)
            {
                TempData["Success"] = $"Order status updated to {orderStatus} successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update order status.";
            }

            return RedirectToAction("AdminOrderDetails", new { id });
        }
    }
}