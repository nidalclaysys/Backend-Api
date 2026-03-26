namespace MyWebAppApi.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SendId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SendAt { get; set; }
        public bool IsRead {  get; set; }

        public byte MessageType { get; set; }
        public string FileName { get; set; }
    }
}
