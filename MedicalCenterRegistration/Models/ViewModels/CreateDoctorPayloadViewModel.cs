// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MedicalCenterRegistration.Enums;

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class CreateDoctorPayloadViewModel
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
        [StringLength(100, MinimumLength = 5)]
        [DisplayName("Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [DisplayName("Hasło")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
            ErrorMessage = "Hasło musi zawierać co najmniej 6 znaków, w tym jedną wielką literę, jedną małą literę, jedną cyfrę i jeden znak specjalny.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Imię")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Opis lekarza jest wymagany.")]
        [StringLength(500, MinimumLength = 1)]
        [DisplayName("Opis lekarza")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Nazwisko")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
        [DataType(DataType.Date)]
        [DisplayName("Data urodzenia")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Płeć jest wymagana.")]
        [EnumDataType(typeof(Sex), ErrorMessage = "Nieprawidłowa płeć.")]
        [DisplayName("Płeć")]
        public Sex Sex { get; set; }

        [Required(ErrorMessage = "Zdjęcie jest wymagane.")]
        [DisplayName("Zdjęcie profilowe")]
        public IFormFile ImageFile { get; set; }
    }
}
