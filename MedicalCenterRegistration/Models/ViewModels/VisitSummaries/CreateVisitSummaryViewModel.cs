// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using MedicalCenterRegistration.Validation;

namespace MedicalCenterRegistration.Models.ViewModels.VisitSummaries
{
    public class CreateVisitSummaryViewModel
    {
        [Required(ErrorMessage = "Podsumowanie wizyty jest wymagane")]
        [Display(Name = "Podsumowanie wizyty i zalecenia")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Załączniki")]
        [MaxFileCount(5, ErrorMessage = "Można dodać maksymalnie 5 plików.")]
        public List<IFormFile>? UploadedFiles { get; set; }

        public int VisitId { get; set; }
        public int Id { get; set; }
    }
}
