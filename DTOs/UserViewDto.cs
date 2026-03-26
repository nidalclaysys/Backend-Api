using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebAppApi.DTOs
{
    public class UsersViewDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string? UserName { get; set; }

        [Required]

        public string? FirstName { get; set; }

        [Required]

        public string? LastName { get; set; }


        [Required]

        public string? Phone { get; set; }

        public string? ProfileImage { get; set; }

        [Required]

        public string? Role { get; set; }

        public bool IsActive { get; set; }

    }

}
