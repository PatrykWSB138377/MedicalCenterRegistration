// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MedicalCenterRegistration.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalCenterRegistration.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Imię")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Nazwisko")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu. Wprowadź poprawny numer telefonu.")]
        [DisplayName("Numer telefonu")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Pesel jest wymagany.")]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Numer PESEL musi składać się z 11 cyfr.")]
        [DisplayName("Numer PESEL")]
        public string PeselNumber { get; set; }

        [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
        [DataType(DataType.Date)]
        [DisplayName("Data urodzenia")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Płeć jest wymagana.")]
        [EnumDataType(typeof(Sex), ErrorMessage = "Nieprawidłowa płeć.")]
        [DisplayName("Płeć")]
        public Sex Sex { get; set; }

        // ADDRESS DATA

        [Required(ErrorMessage = "Ulica jest wymagana.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Ulica")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Numer domu jest wymagany.")]
        [StringLength(10, MinimumLength = 1)]
        [DisplayName("Numer domu")]
        public string HouseNumber { get; set; }

        [Required(ErrorMessage = "Województwo jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Województwo")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Powiat mieszkania jest wymagany.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Powiat")]
        public string District { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Nieprawidłowy kod pocztowy. Wprowadź kod w formacie XX-XXX.")]
        [DisplayName("Kod pocztowy")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Miejscowość jest wymagana.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Miejscowość")]
        public string City { get; set; }


        // OTHER
        [ValidateNever]
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }


        public DateTime CreatedAt { get; set; }

    }
}
