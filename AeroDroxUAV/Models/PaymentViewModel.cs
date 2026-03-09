using System.ComponentModel.DataAnnotations;

namespace AeroDroxUAV.Models
{
    public class PaymentViewModel
    {
        public int UserId { get; set; }
        public double TotalAmount { get; set; }
        public int ItemCount { get; set; }
        
        [Required(ErrorMessage = "Please select a payment method")]
        public string PaymentMethod { get; set; } = string.Empty;
        
        // Shipping Information
        [Required(ErrorMessage = "Shipping address is required")]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        public string ShippingCity { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "State is required")]
        public string ShippingState { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Pin code is required")]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Please enter a valid 6-digit pin code")]
        public string ShippingPinCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Please enter a valid 10-digit mobile number")]
        public string ShippingPhone { get; set; } = string.Empty;
        
        // Card Details (for Credit/Debit Card)
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? Cvv { get; set; }
        
        // UPI Details
        public string? UpiId { get; set; }
        
        // Net Banking
        public string? BankName { get; set; }
    }
}