using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.ViewModels
{
    public class DoctorSpecializationCreateViewModel
    {
        [Required(ErrorMessage = "Wybierz lekarza.")]
        [Display(Name = "Lekarz")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Wybierz specjalizację.")]
        [Display(Name = "Specjalizacja")]
        public int SpecializationId { get; set; }

        // Listy do wyświetlenia w dropdownach
        public List<SelectListItem>? Doctor { get; set; }
        public List<SelectListItem>? Specialization { get; set; }
    }
}
