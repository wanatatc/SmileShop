using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmileShop.Authorization.DTOs;
using SmileShop.Authorization.Services;
using System.Threading.Tasks;

namespace SmileShop.Authorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet("role")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _auth.GetRoles();
            return Ok(result);
        }

        /// <summary>
        /// Add Role
        /// </summary>
        /// <remarks>Add new role here</remarks>
        /// <param name="newRole">RoleName: [User, Manager, Admin, Developer]</param>
        /// <return>abcdefg</return>
        [HttpPost("role/add")]
        [Authorize]
        public async Task<IActionResult> AddRole([BindRequired] RoleDtoAdd newRole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _auth.AddRole(newRole);
            return Ok(result);
        }

        [HttpPut("role/update")]
        [Authorize]
        public async Task<IActionResult> UpdateRole(string id, RoleDtoAdd newRole)
        {
            var result = await _auth.UpdateRole(id, newRole);
            return Ok(result);
        }

        [HttpDelete("role/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var result = await _auth.DeleteRole(id);
            return Ok(result);
        }

        [HttpGet("userroles")]
        public IActionResult UserRoles()
        {
            var result = _auth.GetUserRoles();
            return Ok(result);
        }

        [HttpGet("user")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _auth.GetUsers();
            return Ok(result);
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _auth.GetUserById(id);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto login)
        {
            var result = await _auth.Login(login);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto register)
        {
            var result = await _auth.Register(register);
            return Ok(result);
        }

        [HttpPost("assignrole")]
        public async Task<IActionResult> AssignRole(AssignRoleDto assignRole)
        {
            var result = await _auth.AssignRole(assignRole);
            return Ok(result);
        }

        [HttpPost("renew")]
        [Authorize]
        public async Task<IActionResult> Renew()
        {
            var result = await _auth.Renew();

            return Ok(result);
        }
    }
}