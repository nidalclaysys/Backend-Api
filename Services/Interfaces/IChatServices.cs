using MyWebAppApi.Models;

using MyWebAppApi.DTOs;

namespace MvcApp.Services.Interfaces
{
    public interface IChatServices
    {
        Task<IEnumerable<SearchResult>> GetSearchResults(string term, int userId);

        Task<IEnumerable<ChatRoomDto>> GetSessions(int id);

        Task<int> GetOrCreateSession(int userId, int otherUserId);

        Task<IEnumerable<Messages>> GetMessagesAsync(int chatId);

        Task<ApiResponse<string>> UploadChatFile(int chatId, IFormFile file);


    }
}
