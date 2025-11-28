// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using MedicalCenterRegistration.Enums;

namespace MedicalCenterRegistration.Models.ViewModels.Visits
{
    public class VisitDetailsViewModel
    {
        public int VisitId { get; set; }
        public Status Status { get; set; }
        public DateOnly VisitScheduleDate { get; set; }
        public TimeOnly VisitTimeStart { get; set; }
        public bool IsCancellable => Status == Status.Pending && 
            (VisitScheduleDate > DateOnly.FromDateTime(DateTime.Now) || 
             (VisitScheduleDate == DateOnly.FromDateTime(DateTime.Now) && VisitTimeStart > TimeOnly.FromDateTime(DateTime.Now)));
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
