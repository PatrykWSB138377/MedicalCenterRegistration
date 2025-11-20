// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Models
{
    public class Specialization
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nazwa specjalizacji jest wymagana.")]
        [Display(Name = "Nazwa specjalizacji")]
        public string Name { get; set; }
    }
}
