// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress]
        [Display(Name = "Adres e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }



        [Required(ErrorMessage = "Rola użytkownika jest wymagana.")]
        [Display(Name = "Rola użytkownika")]
        public string Role { get; set; }

        // Opcjonalnie: zdjęcie profilowe
        public IFormFile? ImageFile { get; set; }
    }
}
