using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }



        [Required(ErrorMessage = "Rola użytkownika jest wymagana.")]
        public string Role { get; set; }

        // Opcjonalnie: zdjęcie profilowe
        public IFormFile? ImageFile { get; set; }
    }
}
