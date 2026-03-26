namespace MyWebAppApi.DTOs
{
    public class ChatSession
    {
        public int ChatId { get; set; }
        public int OtherUserId { get; set; }

    }

    public class ChatUserInfo
    {
        public string Name { get; set; } = string.Empty;

        public string ProfilePath { get; set; }= string.Empty;

        public string LastMessage {  get; set; } = string.Empty;
        public string SendedAt { get; set; } = string.Empty;

    }

    public class SearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ChatRoomDto
    {
        public int ChatId { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        
        public string UserImage {  get; set; } = string.Empty;

    }

    public class ChatData
    {
        public string UserName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}
