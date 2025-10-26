using System;
using System.ComponentModel.DataAnnotations;
using MedicalCenterRegistration.Enums;

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class PatientCardViewModel
    {
        [Required(ErrorMessage = "Pole użytkownika jest wymagane.")]
        public string UserId { get; set; }

        [Display(Name = "Adres e-mail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(100,MinimumLength =1, ErrorMessage = "Imię może mieć maksymalnie 100 znaków.")]
        [Display(Name = "Imię")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(100,MinimumLength =1, ErrorMessage = "Nazwisko może mieć maksymalnie 100 znaków.")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data urodzenia")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Płeć jest wymagana.")]
        [Display(Name = "Płeć")]
        public Sex Sex { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Niepoprawny numer telefonu.")]
        [Display(Name = "Numer telefonu")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Numer PESEL jest wymagany.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL musi zawierać dokładnie 11 cyfr.")]
        [Display(Name = "Numer PESEL")]
        public string PeselNumber { get; set; }

        [Required(ErrorMessage = "Ulica jest wymagana.")]
        [StringLength(100, MinimumLength = 1)]
        [Display(Name = "Ulica")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Numer domu jest wymagany.")]
        [Display(Name = "Numer domu")]
        public string HouseNumber { get; set; }

        [Required(ErrorMessage = "Województwo jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [Display(Name = "Województwo")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Powiat jest wymagany.")]
        [StringLength(100, MinimumLength = 1)]
        [Display(Name = "Powiat")]
        public string District { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy musi być w formacie XX-XXX.")]
        [Display(Name = "Kod pocztowy")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [Display(Name = "Miasto")]
        public string City { get; set; }

        public bool ExistsAlready { get; set; } = false;
    }
}
