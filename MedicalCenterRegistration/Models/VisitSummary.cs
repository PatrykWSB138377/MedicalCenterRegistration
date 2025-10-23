// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Models
{
    public class VisitSummary
    {
        public int Id { get; set; }

        public int VisitId { get; set; }
        public Visit? Visit { get; set; }


        [Required]
        [Display(Name = "Podsumowanie wizyty i zalecenia")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty; // Notes from doctor

        [Display(Name = "Załączniki")]
        public List<UserFile> Files { get; set; } = new();

        [Display(Name = "Data utworzenia")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Ostatnia aktualizacja")]
        public DateTime? UpdatedAt { get; set; }
    }
}
