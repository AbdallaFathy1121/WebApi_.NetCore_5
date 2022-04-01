using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppApi.Bl;

namespace WebAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        // Use Dependency Injection
        private IUserService _userService;
        private IMailService _mailService;
        public AuthController(IUserService userService, IMailService mailService)
        {
            _userService = userService;
            _mailService = mailService;
        }


        // api/auth/register => (Body ...)
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.RegisterUserAsync(model);

                if (result.IsSuccess)
                    return Ok(result); // Status Code: 200
                else
                    return BadRequest(result);
            }

            return BadRequest("Some Properties are not valid"); // Status Code: 400
        }

        // api/auth/login => (Body ...)
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUserAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result); // Status Code: 200
                }
                else
                    return BadRequest(result);
            }

            return BadRequest("Some Properties are not valid"); // Status Code: 400
        }

        // api/auth/test
        [Authorize]
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok("Login succiss");
        }

        // api/auth/email
        [HttpPost("Email")]
        public async Task<IActionResult> EmailAsync([FromForm]MailViewModel model)
        {
            await _mailService.SendEmailAsync(model.ToEmail, model.Subject, model.Body);
            return Ok("Email Send Success");
        }


    }
}
