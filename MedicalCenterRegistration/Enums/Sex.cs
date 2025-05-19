// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Enums
{
    public enum Sex
    {
        [Display(Name = "Mężczyzna")]
        Male,
        [Display(Name = "Kobieta")]
        Female,
        [Display(Name = "Inna")]
        Other
    }
}
