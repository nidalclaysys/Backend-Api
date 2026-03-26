

using MvcApp.Services.Interfaces;
using MyWebAppApi.DTOs;
using MyWebAppApi.Models;
using MyWebAppApi.Repository;
using MyWebAppApi.Repository.Interfaces;

namespace MvcApp.Services
{
    public class ChatServices : IChatServices
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatServices> _logger;
        private readonly IWebHostEnvironment _env;


        public ChatServices(IChatRepository chatRepository,ILogger<ChatServices> logger,IWebHostEnvironment webHostEnvironment)
        { 
            _chatRepository = chatRepository;
            _logger = logger;
            _env = webHostEnvironment;

        }

        public async Task<IEnumerable<ChatRoomDto>> GetSessions(int id)
        {
            _logger.LogInformation("Fetching chat sessions for UserId: {UserId}", id);

            var sessions = await _chatRepository.GetAllExistingSession(id);

            var sessionRoom = new List<ChatRoomDto>();

            foreach (var session in sessions)
            {
                var userData = await _chatRepository.GetChatData(session.OtherUserId) ?? throw new Exception("User Not Found");

                sessionRoom.Add(new ChatRoomDto
                {
                    ChatId = session.ChatId,
                    OtherUserId = session.OtherUserId,
                    OtherUserName = userData.UserName,
                    UserImage = userData.ImagePath
                });

                _logger.LogDebug("Session created: ChatId={ChatId}, OtherUserId={OtherUserId}, OtherUserName={OtherUserName}",
                   session.ChatId, session.OtherUserId, userData.UserName ?? "unknown");
            }
            
            _logger.LogInformation("Retrieved {Count} sessions for UserId: {UserId}", sessionRoom.Count, id);
            return sessionRoom;
        }

        public async Task<IEnumerable<SearchResult>> GetSearchResults(string term,int userId)
        {
            _logger.LogInformation("Searching users with term '{Term}' for UserId: {UserId}", term, userId);

            var result = await _chatRepository.SearchUsers(term, userId);
            _logger.LogInformation("Found {Count} search results for term '{Term}'", result.Count(), term);
            return result;

        }

        public async Task<int> GetOrCreateSession (int userId,int otherUserId)
        {
            _logger.LogInformation("Getting or creating chat session between UserId: {UserId} and OtherUserId: {OtherUserId}",
               userId, otherUserId);

            var sessionId = await _chatRepository.GetOrCreateChatSession(userId, otherUserId);
            _logger.LogInformation("Chat session established with SessionId: {SessionId}", sessionId);
            return sessionId;
        }

        public async Task<IEnumerable<Messages>> GetMessagesAsync(int chatId)
        {
            _logger.LogInformation("Fetching messages for ChatId: {ChatId}", chatId);

            var messages = await _chatRepository.GetMessagesAsync(chatId);
            _logger.LogInformation("Retrieved {Count} messages for ChatId: {ChatId}", messages.Count(), chatId);
            return messages;
        }

        public async Task<ApiResponse<string>> UploadChatFile(int chatId, IFormFile file)
        {
            _logger.LogInformation("New file added to chat {chatId}", chatId);

            string folder = Path.Combine(_env.WebRootPath, "uploads", "chatFiles", chatId.ToString());
            Directory.CreateDirectory(folder);

            string safeFileName = Path.GetFileName(file.FileName);
            string fullPath = Path.Combine(folder, safeFileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            string relativePath = $"/uploads/chatFiles/{chatId}/{safeFileName}";

            _logger.LogInformation("File added successfully for chat {chatId}", chatId);

            return ApiResponseBuilder.Success(relativePath, "File uploaded successfully");
        }

    }
}