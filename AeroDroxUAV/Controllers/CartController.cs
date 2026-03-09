using AeroDroxUAV.Models;
using AeroDroxUAV.Services;
using Microsoft.AspNetCore.Mvc;

namespace AeroDroxUAV.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IDroneService _droneService;
        private readonly IAccessoriesService _accessoriesService;

        public CartController(
            ICartService cartService,
            IDroneService droneService,
            IAccessoriesService accessoriesService)
        {
            _cartService = cartService;
            _droneService = droneService;
            _accessoriesService = accessoriesService;
        }

        // Security Helpers
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        
        private int? GetCurrentUserId()
        {
            // FIX: Get the user ID as an integer directly
            var userId = HttpContext.Session.GetInt32("UserId");
            return userId;
        }

        // View Cart - Login Required
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                // If user ID is invalid, clear session and redirect to login
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _cartService.GetUserCartAsync(userId.Value);
            var cartTotal = await _cartService.GetCartTotalAsync(userId.Value);
            var itemCount = await _cartService.GetCartItemCountAsync(userId.Value);

            ViewBag.CartTotal = cartTotal;
            ViewBag.ItemCount = itemCount;
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
            ViewBag.IsLoggedIn = IsLoggedIn();

            return View(cartItems);
        }

        // Add to Cart - AJAX endpoint
        [HttpPost]
        public async Task<IActionResult> AddToCart(int? droneId, int? accessoryId, int quantity = 1)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "Please login to add items to cart", redirectUrl = "/Account/Login" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Invalid user session", redirectUrl = "/Account/Login" });
            }

            if (!droneId.HasValue && !accessoryId.HasValue)
                return Json(new { success = false, message = "Invalid product" });

            try
            {
                await _cartService.AddToCartAsync(userId.Value, droneId, accessoryId, quantity);
                
                var itemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                
                return Json(new { 
                    success = true, 
                    message = "Item added to cart successfully",
                    itemCount = itemCount
                });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while adding to cart" });
            }
        }

        // Update Quantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "Please login" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Invalid user session" });
            }

            try
            {
                await _cartService.UpdateQuantityAsync(cartItemId, quantity);
                
                var cartTotal = await _cartService.GetCartTotalAsync(userId.Value);
                var itemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                
                return Json(new { 
                    success = true, 
                    message = "Cart updated successfully",
                    cartTotal = cartTotal,
                    itemCount = itemCount
                });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Remove from Cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "Please login" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Invalid user session" });
            }

            try
            {
                await _cartService.RemoveFromCartAsync(cartItemId);
                
                var cartTotal = await _cartService.GetCartTotalAsync(userId.Value);
                var itemCount = await _cartService.GetCartItemCountAsync(userId.Value);
                
                return Json(new { 
                    success = true, 
                    message = "Item removed from cart",
                    cartTotal = cartTotal,
                    itemCount = itemCount
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while removing item" });
            }
        }

        // Clear Cart
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "Please login" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Invalid user session" });
            }

            try
            {
                await _cartService.ClearCartAsync(userId.Value);
                
                return Json(new { success = true, message = "Cart cleared successfully" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while clearing cart" });
            }
        }

        // Get Cart Count for Navbar
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            if (!IsLoggedIn())
                return Json(0);

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Json(0);

            var count = await _cartService.GetCartItemCountAsync(userId.Value);
            return Json(count);
        }
    }
}