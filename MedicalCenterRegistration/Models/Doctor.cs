// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MedicalCenterRegistration.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Imię")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(100, MinimumLength = 1)]
        [DisplayName("Nazwisko")]
        public required string LastName { get; set; }

        public required string UserId { get; set; }
        public required IdentityUser User { get; set; }


        public DateTime CreatedAt { get; set; }

    }
}
