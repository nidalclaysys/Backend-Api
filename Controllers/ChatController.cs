using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Services.Interfaces;
using MyWebAppApi.Helper;

namespace MvcApp.Controllers
{
    [Authorize]
    [Route("api/chat")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IChatServices _chatServices; 
        private readonly IUserFinder _userFinder;

        public ChatController(IChatServices chatServices, IUserFinder userFinder)
        {
            _userFinder = userFinder;
            _chatServices = chatServices; 
        }

        [HttpGet("users/search")]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var userId = _userFinder.GetId();

            var searchResults = await _chatServices.GetSearchResults(term, userId);

            return Ok(searchResults);
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSession()
        {
            var userId = _userFinder.GetId();
            var sessions = await _chatServices.GetSessions(userId);
            return Ok(sessions);
        }

        [HttpPost("sessions/{otherUserId}")]

        public async Task<IActionResult> CreateOrGetSession(int otheruserid)
        {
            var userId = _userFinder.GetId();
            var result = await _chatServices.GetOrCreateSession(userId, otheruserid);
            return Ok(result);

        }

        [HttpGet("sessions/{chatId}/messages")]

        public async Task<IActionResult> GetMessage(int chatId)
        {
            var messages = await _chatServices.GetMessagesAsync(chatId);
            return Ok(messages);
        }

        [HttpPost("file/{chatId}")]
        public async Task<IActionResult> SaveFile(int chatId,IFormFile file)
        {
            if (file == null) return BadRequest("No file provided");

            var result = await _chatServices.UploadChatFile(chatId, file);

            if (result.IsSuccess)
            {
                return Ok(result); 
            }
            return BadRequest(result.Message);


        }

      
    }
}
