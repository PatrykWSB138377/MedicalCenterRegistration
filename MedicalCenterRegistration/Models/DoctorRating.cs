// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Models
{
    public class DoctorRating
    {
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Ocena musi być w zakresie od 1 do 5.")]
        [Display(Name = "Ocena (1-5)")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Komentarz może mieć maksymalnie 500 znaków.")]
        [Display(Name = "Komentarz")]
        public string? Comment { get; set; }

        [Display(Name = "Data wystawienia")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relacje
        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; } = null!; 

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; } = null!;   
    }
}
