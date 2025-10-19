// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.ViewModels.Visits
{
    public class SpecializationWithDoctorCount
    {
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public int DoctorCount { get; set; }
    }

    public class ChooseDoctorSpecializationViewModel
    {
        public List<SpecializationWithDoctorCount> Specializations { get; set; }
    }
}
