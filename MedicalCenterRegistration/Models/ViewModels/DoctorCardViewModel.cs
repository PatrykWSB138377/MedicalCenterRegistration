// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class DoctorCardViewModel

    {
        public int DoctorId { get; set; }
        public string? Description { get; set; }
        public string Image { get; set; }
        public string FullName { get; set; } = null!;
        public string Specializations { get; set; } = null!;
        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }
        public bool UserHasRated { get; set; }
        public int? UserRatingId { get; set; }
        public bool CanRate { get; set; }   

    }
}
