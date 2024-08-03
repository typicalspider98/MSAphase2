using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class RegisterDto
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Username is invalid,,,, can only contain letters or digits.")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}