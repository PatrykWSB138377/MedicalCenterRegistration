// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Models
{
    public class PublicImage
    {
        public int Id { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public string FileName { get; set; }
    }
}
