﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MedicalCenterRegistration.Enums;
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

        [Required(ErrorMessage = "Opis lekarza jest wymagany.")]
        [StringLength(500, MinimumLength = 1)]
        [DisplayName("Opis lekarza")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
        [DataType(DataType.Date)]
        [DisplayName("Data urodzenia")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Płeć jest wymagana.")]
        [EnumDataType(typeof(Sex), ErrorMessage = "Nieprawidłowa płeć.")]
        [DisplayName("Płeć")]
        public Sex Sex { get; set; }


        public int ImageId { get; set; }
        public virtual PublicImage Image { get; set; }

        public required string UserId { get; set; }
        public IdentityUser? User { get; set; }


        public DateTime CreatedAt { get; set; }

    }
}
