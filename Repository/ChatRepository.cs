using Microsoft.Data.SqlClient;
using MyWebAppApi.DTOs;
using MyWebAppApi.Models;
using MyWebAppApi.Repository.Interfaces;

namespace MyWebAppApi.Repository
{
    public class ChatRepository : BaseRepository, IChatRepository
    {
        private readonly ILogger<ChatRepository> _logger;
        public ChatRepository(IConfiguration configuration, ILogger<ChatRepository> logger) : base(configuration)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<SearchResult>> SearchUsers(string name,int id)
        {
            _logger.LogInformation("Searching for name={name}, id={id}", name, id);

            string sql = "SELECT UserId,FirstName,LastName FROM App.Users WHERE (LOWER(FirstName) LIKE LOWER(@name) OR LOWER(LastName) LIKE LOWER(@name)) AND UserId != @id;";

            using var conn = GetConnection();

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@name",$"%{name}%");
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();

            var read = await cmd.ExecuteReaderAsync();

            var list = new List<SearchResult>();

            while (await read.ReadAsync())
            {
                list.Add(new SearchResult
                {
                    Id = Convert.ToInt32(read["UserId"]),
                    Name = read.GetString(read.GetOrdinal("FirstName"))
                         + " " + read.GetString(read.GetOrdinal("LastName"))
                });
            }

            _logger.LogInformation("list {@list}",list);
            return list;
        }

        public async Task<IEnumerable<ChatSession>> GetAllExistingSession(int userid)
        {
            string getSql = "SELECT Id,User1,User2 FROM App.Chats WHERE User1 = @u1 OR User2 = @u1;";

            using var conn = GetConnection();

            using var cmd = new SqlCommand(getSql, conn);

            cmd.Parameters.AddWithValue("@u1", userid);

            await conn.OpenAsync();


            var read = await cmd.ExecuteReaderAsync();

            var chatRooms = new List<ChatSession>();

            while (await read.ReadAsync())
            {
                int chatId = Convert.ToInt32(read["Id"]);
                int user1 = Convert.ToInt32(read["User1"]);
                int user2 = Convert.ToInt32(read["User2"]);

                int otheruser = (user1 == userid) ? user2 : user1;

                chatRooms.Add(
                    new ChatSession
                    {
                        ChatId = chatId,
                        OtherUserId = otheruser
                    });
            }
            return chatRooms ;
        }

        public async Task<ChatData?> GetChatData(int userId)
        {
            string sql = "SELECT FirstName,LastName,ProfileImagePath FROM App.Users WHERE UserId = @id;";

            using var conn = GetConnection();

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", userId);
            await conn.OpenAsync();

            var read = await cmd.ExecuteReaderAsync();

            if (await read.ReadAsync())
            {
                string fn = Convert.ToString(read["FirstName"]);
                string ln = Convert.ToString(read["LastName"]);
                string img = read["ProfileImagePath"] == DBNull.Value ? null : Convert.ToString(read["ProfileImagePath"]);


                return new ChatData
                {
                    UserName = fn + " " + ln,
                    ImagePath = img
                };
            }

            _logger.LogWarning("No user found for userId: {userId}", userId);
            return null; 


        }



        public async Task<int> GetOrCreateChatSession(int user1, int user2)
        {
            if (user1 > user2)
            {
                (user1, user2) = (user2, user1);
            }
            _logger.LogInformation("chating requestst between {u1} and {u2}", user1, user2);

            string getSql = "SELECT Id FROM App.Chats WHERE User1 = @u1 AND User2 = @u2;";

            using var conn = GetConnection();

            using var cmd = new SqlCommand(getSql, conn);

            cmd.Parameters.AddWithValue("@u1", user1);
            cmd.Parameters.AddWithValue("@u2", user2);
            await conn.OpenAsync();


            var result = await cmd.ExecuteScalarAsync();

           if(result != null) return Convert.ToInt32(result);

            string createSql = "INSERT INTO App.Chats(User1,User2) OUTPUT INSERTED.id VALUES(@u1,@u2) ";

            using var newcmd = new SqlCommand(createSql, conn);

            newcmd.Parameters.AddWithValue("@u1", user1);
            newcmd.Parameters.AddWithValue("@u2", user2);

            return Convert.ToInt32( await newcmd.ExecuteScalarAsync());

        }

        public async Task<int> SaveMessage(int chatId, int senderId, string message, int messageType = 0, string fileName = null)
        {
            string sql = @"INSERT INTO App.Messages(ChatId, SendId, message, MessageType, FileName) 
                   OUTPUT INSERTED.Id 
                   VALUES(@cid, @sid, @msg, @mtype, @fname)";

            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@cid", chatId);
            cmd.Parameters.AddWithValue("@sid", senderId);
            cmd.Parameters.AddWithValue("@msg", message);
            cmd.Parameters.AddWithValue("@mtype", messageType);
            cmd.Parameters.AddWithValue("@fname", (object)fileName ?? DBNull.Value);

            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<IEnumerable<Messages>> GetMessagesAsync(int chatId)
        {
            string sql = "SELECT * FROM App.Messages WHERE ChatId = @cid ORDER BY SendAt ASC";

            using var conn = GetConnection();

            using var cmd = new SqlCommand(sql, conn);


            cmd.Parameters.AddWithValue("@cid", chatId);

            await conn.OpenAsync();

            var read = await cmd.ExecuteReaderAsync();

            var messages = new List<Messages>();

            while (await read.ReadAsync())
            {
                messages.Add(new Messages
                {
                    Id = Convert.ToInt32(read["Id"]),
                    ChatId = Convert.ToInt32(read["ChatId"]),
                    SendId = Convert.ToInt32(read["SendId"]),
                    Message = Convert.ToString(read["Message"]),
                    SendAt = Convert.ToDateTime(read["SendAt"]),
                    IsRead = Convert.ToBoolean(read["IsRead"]),
                    MessageType = Convert.ToByte(read["MessageType"]),
                    FileName = read["FileName"] != DBNull.Value ? Convert.ToString(read["FileName"]) : null
                });
            }

            return messages;
        }

        public async Task MarkMessageAsRead(int messageId, int userId)
        {
            string sql = "UPDATE App.Messages SET IsRead = 1 WHERE Id = @mid AND ChatId IN (SELECT Id FROM App.Chats WHERE User1 = @uid OR User2 = @uid);";

            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@mid", messageId);
            cmd.Parameters.AddWithValue("@uid", userId);

            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No message updated for messageId={mid}, userId={uid}", messageId, userId);
            }
            else
            {
                _logger.LogInformation("Message {mid} marked as read by user {uid}", messageId, userId);
            }
        }

    }
}
