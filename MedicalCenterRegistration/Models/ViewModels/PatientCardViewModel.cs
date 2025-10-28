using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MedicalCenterRegistration.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class PatientCardViewModel
    {
        [Required(ErrorMessage = "Wybierz użytkownika.")]
        [DisplayName("Użytkownik z rolą pacjenta")]
        public string SelectedUserId { get; set; }

        public List<SelectListItem> AvailableUsers { get; set; } = new();

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Imię")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
        [DisplayName("Numer telefonu")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "PESEL jest wymagany.")]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "PESEL musi mieć 11 cyfr.")]
        [DisplayName("PESEL")]
        public string PeselNumber { get; set; }

        [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
        [DataType(DataType.Date)]
        [DisplayName("Data urodzenia")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Płeć jest wymagana.")]
        [DisplayName("Płeć")]
        public Sex Sex { get; set; }

        [Required(ErrorMessage = "Ulica jest wymagana.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Ulica")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Numer domu jest wymagany.")]
        [DisplayName("Numer domu")]
        public string HouseNumber { get; set; }

        [Required(ErrorMessage = "Województwo jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Województwo")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Powiat jest wymagany.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Powiat")]
        public string District { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Nieprawidłowy kod pocztowy (XX-XXX).")]
        [DisplayName("Kod pocztowy")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Miejscowość jest wymagana.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Miejscowość")]
        public string City { get; set; }
    }
}
