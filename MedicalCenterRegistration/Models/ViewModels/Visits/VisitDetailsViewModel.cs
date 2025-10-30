// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Models.ViewModels.Visits
{
    public class VisitDetailsViewModel
    {
        [Display(Name = "Imię i nazwisko lekarza")]
        public string DoctorFullName { get; set; }
        [Display(Name = "Zdjęcie lekarza")]
        public PublicImage DoctorImage { get; set; }
        [Display(Name = "Specjalizacje lekarza")]
        public List<string> DoctorSpecializations { get; set; }
        [Display(Name = "Imię i nazwisko pacjenta")]
        public string PatientFullName { get; set; }
        [Display(Name = "Data wizyty")]
        public DateOnly VisitDate { get; set; }
        [Display(Name = "Godziny wizyty")]
        public string FormattedVisitTime { get; set; }
        [Display(Name = "Podsumowanie wizyty i zalecenia lekarza")]
        public string Description { get; set; }
        [Display(Name = "Załączniki do wizyty")]
        public List<UserFile> Files { get; set; }
        [Display(Name = "Data utworzenia")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Ostatnia aktualizacja")]
        public DateTime? UpdatedAt { get; set; }

        public string? GoBackUrl { get; set; }
    }
}
