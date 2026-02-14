using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AeroDroxUAV.Controllers
{
    // Apply anti-caching
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ContactController : Controller
    {
        private readonly IConfiguration _configuration;

        public ContactController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /Contact/Index
        public IActionResult Index()
        {
            ViewData["Title"] = "Contact Us";
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
            return View();
        }

        // POST: /Contact/Submit
        [HttpPost]
        public async Task<IActionResult> Submit(string name, string email, string phone, string company, string subject, string message, bool newsletter = false)
        {
            var errors = new List<string>();

            // Validate Name
            if (string.IsNullOrEmpty(name))
            {
                errors.Add("Name is required.");
            }
            else if (!Regex.IsMatch(name, @"^[a-zA-Z\s]{2,50}$"))
            {
                errors.Add("Name must be 2-50 characters and contain only letters and spaces.");
            }

            // Validate Email
            if (string.IsNullOrEmpty(email))
            {
                errors.Add("Email is required.");
            }
            else if (!IsValidEmail(email))
            {
                errors.Add("Please enter a valid email address.");
            }

            // Validate Phone (optional but must be valid if provided)
            if (!string.IsNullOrEmpty(phone))
            {
                // Remove any formatting for validation
                string cleanPhone = Regex.Replace(phone, @"[\s\-\(\)\+]", "");
                
                // Check if it's a valid Indian phone number
                if (!Regex.IsMatch(cleanPhone, @"^[6-9][0-9]{9}$") && !Regex.IsMatch(cleanPhone, @"^[0-9]{10,12}$"))
                {
                    errors.Add("Please enter a valid phone number (10 digits Indian number or with country code).");
                }
            }

            // Validate Company (optional but must be valid if provided)
            if (!string.IsNullOrEmpty(company) && (company.Length < 2 || company.Length > 100))
            {
                errors.Add("Company name must be between 2 and 100 characters.");
            }

            // Validate Subject
            if (string.IsNullOrEmpty(subject))
            {
                errors.Add("Subject is required.");
            }
            else
            {
                string[] validSubjects = { "general", "sales", "support", "service", "training", "partnership", "other" };
                if (!validSubjects.Contains(subject))
                {
                    errors.Add("Please select a valid subject.");
                }
            }

            // Validate Message
            if (string.IsNullOrEmpty(message))
            {
                errors.Add("Message is required.");
            }
            else if (message.Length < 10)
            {
                errors.Add("Message must be at least 10 characters long.");
            }
            else if (message.Length > 2000)
            {
                errors.Add("Message cannot exceed 2000 characters.");
            }

            // If there are validation errors, return to form with errors
            if (errors.Any())
            {
                ViewBag.ErrorMessage = string.Join(" ", errors);
                
                // Preserve form data
                ViewBag.FormData = new
                {
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Company = company,
                    Subject = subject,
                    Message = message,
                    Newsletter = newsletter
                };
                
                ViewData["Title"] = "Contact Us";
                ViewBag.IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
                ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
                return View("Index");
            }

            try
            {
                // Prepare email
                string toEmail = "dnyaneshkshatriya7123@gmail.com";
                string subjectText = $"Website Contact Form: {GetSubjectDisplay(subject)} - {name}";
                
                // Escape any potential HTML in user input
                string safeName = WebUtility.HtmlEncode(name);
                string safeEmail = WebUtility.HtmlEncode(email);
                string safePhone = WebUtility.HtmlEncode(string.IsNullOrEmpty(phone) ? "Not provided" : phone);
                string safeCompany = WebUtility.HtmlEncode(string.IsNullOrEmpty(company) ? "Not provided" : company);
                string safeMessage = WebUtility.HtmlEncode(message);
                string newsletterStatus = newsletter ? "Yes" : "No";
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string subjectDisplay = GetSubjectDisplay(subject);
                
                string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #1a2980;'>New Contact Form Submission</h2>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Name:</strong></td>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{safeName}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Email:</strong></td>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><a href='mailto:{email}'>{safeEmail}</a></td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Phone:</strong></td>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{safePhone}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Company:</strong></td>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{safeCompany}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Subject:</strong></td>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{subjectDisplay}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'><strong>Newsletter:</strong></td>
                                <td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{newsletterStatus}</td>
                            </tr>
                        </table>
                    </div>
                    <div style='margin-top: 20px;'>
                        <h3 style='color: #1a2980;'>Message:</h3>
                        <div style='background-color: #f0f8ff; padding: 15px; border-left: 4px solid #26d0ce; border-radius: 5px;'>
                            <p style='white-space: pre-line;'>{safeMessage}</p>
                        </div>
                    </div>
                    <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; color: #6c757d; font-size: 12px;'>
                        <p>This email was sent from the contact form on AeroDrox UAV website.</p>
                        <p>Timestamp: {timestamp}</p>
                    </div>
                </body>
                </html>";

                // Send email using SMTP
                await SendEmailAsync(toEmail, subjectText, body, email);

                ViewBag.SuccessMessage = "Thank you for contacting us! We have received your message and will get back to you within 24 hours.";
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging)
                Console.WriteLine($"Error sending email: {ex.Message}");
                ViewBag.ErrorMessage = "Sorry, there was an error sending your message. Please try again later or contact us directly at support.in@aerodrox.com";
            }

            ViewData["Title"] = "Contact Us";
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
            ViewBag.Role = HttpContext.Session.GetString("Role") ?? "";
            return View("Index");
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, string senderEmail)
        {
            // Get SMTP configuration from appsettings.json or use default
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername, "AeroDrox UAV Contact Form"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                // Add CC to sender for confirmation
                if (!string.IsNullOrEmpty(senderEmail))
                {
                    mailMessage.CC.Add(new MailAddress(senderEmail, "Sender Copy"));
                }

                await client.SendMailAsync(mailMessage);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && 
                       email.Length <= 254 && 
                       Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            }
            catch
            {
                return false;
            }
        }

        private string GetSubjectDisplay(string subject)
        {
            return subject switch
            {
                "general" => "General Inquiry",
                "sales" => "Sales & Pricing",
                "support" => "Technical Support",
                "service" => "Service Request",
                "training" => "Training",
                "partnership" => "Partnership",
                "other" => "Other",
                _ => subject
            };
        }
    }
}