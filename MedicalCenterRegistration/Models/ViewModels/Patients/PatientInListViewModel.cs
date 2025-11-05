// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.ViewModels.Patients
{
    public enum PatientsTableMode
    {
        Default,
        SelectForVisit
    }

    public class PatientInListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public object UserEmail { get; set; }
        public string CreatedAt { get; set; }

        public PatientsTableMode mode = PatientsTableMode.Default;
    }
}
