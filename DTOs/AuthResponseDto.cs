namespace MyWebAppApi.DTOs
{
    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public bool IsAdmin { get; set; }
        public string? UserName { get; set; }
    }
}
