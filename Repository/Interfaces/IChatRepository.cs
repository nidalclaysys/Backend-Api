

using MyWebAppApi.DTOs;
using MyWebAppApi.Models;

namespace MyWebAppApi.Repository.Interfaces
{
    public interface IChatRepository
    {
        Task<IEnumerable<SearchResult>> SearchUsers(string username,int id);
        Task<IEnumerable<ChatSession>> GetAllExistingSession(int userid);
        Task<int> GetOrCreateChatSession(int user1, int user2);
        Task<ChatData?> GetChatData(int userId);
        Task<int> SaveMessage(int chatId, int senderId, string message, int messageType = 0, string fileName = null);
        Task<IEnumerable<Messages>> GetMessagesAsync(int chatId);
        Task MarkMessageAsRead(int messageId, int userId);
    }
}
