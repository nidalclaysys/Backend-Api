using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Models
{
    public class Credential
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [MaxLength(255)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(225)]
        public string HashedPassword { get; set; } = string.Empty;

        public string? Role { get; set; }

        public int LoginAttempts { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LoginAt { get; set; }

    }
}