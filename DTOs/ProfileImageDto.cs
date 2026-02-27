using MyWebAppApi.Helper.AttributeValidation;
using System.ComponentModel.DataAnnotations;

namespace MyWebAppApi.DTOs
{
    public class ProfileImageDto
    {
        [Required(ErrorMessage = "Please upload an image")]
        [AllowExtensions(new[] { ".jpg", ".jpeg", ".png" })]
        [FileSize(2 * 1024 * 1024)]
        public IFormFile File { get; set; } = null!;

        public string? ExistingImagePath { get; set; }

    }
}
