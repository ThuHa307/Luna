using Luna.Areas.Customer.Models;
using Microsoft.AspNetCore.Mvc;


namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ContactController : Controller
    {
        private readonly ILogger<ContactController> _logger;
        private readonly IMailService _mailService;

        public ContactController(ILogger<ContactController> logger, IMailService mailService)
        {
            _logger = logger;
            _mailService = mailService;
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(MailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _mailService.SendEmailAsync(request);
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
