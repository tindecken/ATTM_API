using Microsoft.AspNetCore.Mvc;
using ATTM_API.Models;
using ATTM_API.Services;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ATTM_API.Models.Entities;
using ATTM_API.Helpers;

namespace ATTM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : TransformResponse
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

            return Transform(response);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordData changePasswordData)
        {
            var response = await _userService.ChangePassword(changePasswordData);
            return Transform(response);
        }
    }
}
