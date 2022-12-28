using Microsoft.AspNetCore.Mvc;
using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;

namespace ATTM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private UserService _userService;
        

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpPost("changepassword")]
        public async Task<ActionResult<JObject>> ChangePassword([FromBody] ChangePasswordData changePasswordData)
        {
            var response = await _userService.ChangePassword(changePasswordData);
            if (response == null) return StatusCode(500, $"Internal server error.");
            var result = response.GetValue("result").ToString();
            if (result.Equals("success"))
            {
                return StatusCode(200, response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }
    }
}
