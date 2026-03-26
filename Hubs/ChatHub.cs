using Microsoft.AspNetCore.SignalR;
using MyWebAppApi.Helper;
using MyWebAppApi.Models;
using MyWebAppApi.Repository.Interfaces;

namespace MvcApp.Hubs
{
    public class ChatHub:Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatRepository _chatRepository;

        private static readonly HashSet<int> _onlineUsers = new HashSet<int>();
        public ChatHub(ILogger<ChatHub> logger,IChatRepository chatRepository)
        {
            _logger = logger;
            _chatRepository = chatRepository;
        }

        public async Task SendMessage(int chatId, int sendId, string message, int messageType = 0, string fileName = null)
        {
            _logger.LogInformation("User {SenderId} sending message to ChatId {ChatId}", sendId, chatId);

            await Clients.Group(chatId.ToString())
                .SendAsync("ReceiveMessage", sendId, message, messageType, fileName);

            await _chatRepository.SaveMessage(chatId, sendId, message, messageType, fileName);
        }

        public async Task JoinChat(int chatId)
        {
            _logger.LogInformation("Connection {ConnectionId} joining ChatId {ChatId}", Context.ConnectionId, chatId);

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            
            _logger.LogDebug("Connection {ConnectionId} successfully joined ChatId {ChatId}", Context.ConnectionId, chatId);

        }

        public async Task MarkMessageAsRead (int messageId,int userId)
        {
            _logger.LogInformation("User {UserId} marking message {MessageId} as read", userId, messageId);

            await _chatRepository.MarkMessageAsRead(messageId, userId);

            await Clients.Group(userId.ToString()).SendAsync("MessageRead", messageId, userId);

        }

        public async Task NotifyTyping(int chatId,int userId)
        {
            await Clients.OthersInGroup(chatId.ToString()).SendAsync("UserTyping",userId);
        }

        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.UserIdentifier;

            _logger.LogWarning("🔌 OnConnected -> UserIdentifier: {UserId}", userIdStr);

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                lock (_onlineUsers)
                {
                    _onlineUsers.Add(userId);
                }

                _logger.LogWarning("✅ User {UserId} added to online list", userId);

                await Clients.All.SendAsync("UserOnline", userId);
            }
            else
            {
                _logger.LogError("❌ UserIdentifier is NULL or INVALID");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdStr = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                lock (_onlineUsers)
                {
                    _onlineUsers.Remove(userId);
                }

                await Clients.All.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public Task<List<int>> GetOnlineUsers()
        {
            lock (_onlineUsers)
            {
                return Task.FromResult(_onlineUsers.ToList());
            }
        }
    }

}
