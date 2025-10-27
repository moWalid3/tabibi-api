using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabibi.EmailService;

namespace Tabibi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController(IEmailSender emailSender) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Send()
        {
            var message = new Message(
                ["mohamedwalidtharwat2020@gmail.com"],
                "Test email subject",
                "This is the content from our email.",
                null);

            await emailSender.SendEmailAsync(message);

            return Ok();
        }

        [Authorize]
        [HttpGet("doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var message = new Message(
                ["mohamedwalidtharwat2020@gmail.com"],
                "Test email subject",
                "This is the content from our email.",
                null);

            await emailSender.SendEmailAsync(message);

            return Ok();
        }
    }
}
