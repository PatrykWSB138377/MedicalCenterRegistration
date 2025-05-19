// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class CreateVisitCreationViewModel
    {
        /* TODO change models to viewmodels */
        public ICollection<Doctor> Doctors { get; set; }
        public Dictionary<int, List<VisitSchedule>> DoctorScheduledVisits { get; set; }
        public Patient Patient { get; set; }
    }
}
