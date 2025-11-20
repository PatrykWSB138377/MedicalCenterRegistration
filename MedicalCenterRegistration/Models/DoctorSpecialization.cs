// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Models
{
    public class DoctorSpecialization
    {
        public int Id { get; set; }
        [Display(Name = "Lekarz")]
        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }

        [Display(Name = "Specjalizacja")]
        public int SpecializationId { get; set; }
        public virtual Specialization Specialization { get; set; }
    }
}
