using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWebAppApi.DTOs;
using MyWebAppApi.Services;
using MyWebAppApi.Services.Interfaces;

namespace MyWebAppApi.Controllers
{
    [Authorize(Roles ="Admin")]
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IUserServices _userServices;

        public AdminController(IAdminService adminService,IUserServices userServices)
        {
            _adminService = adminService;
            _userServices = userServices;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsers();

            return Ok(users);

        }
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserProfile(int id)
        {
            var response = await _userServices.GetUserProfile(id);
            return Ok(response);
        }

        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpdateUserProfile(int id,UpdateProfileDto updateProfileDto)
        {

            var response = await _userServices.UpdateUserProfile(id, updateProfileDto, "Admin");
            return Ok(response);
        }

        [HttpGet("user/{id}/image")]
        public async Task<IActionResult> GetProfileImage([FromRoute] int id)
        {
            var response = await _userServices.GetCurrentProfilePath(id);

            return Ok(response);

        }


        [HttpPost("user/{id}/image")]
        public async Task<IActionResult> Update([FromRoute]int id,[FromForm] ProfileImageDto profileImageDto)
        {
            var response = await _userServices.UpdateImage(id, profileImageDto.File, "Admin");

            return Ok(response);
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _adminService.DeleteUser(id);
            return Ok(response);
        }

        [HttpPut("user/{id}/status")]
        public async Task<IActionResult> ToaggleUserStatus(int id)
        {
            var response = await _adminService.ToaggleUserStatus(id);
            return Ok(response);
        }


    }
}
