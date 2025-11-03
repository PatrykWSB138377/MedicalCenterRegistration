// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.ViewModels.Patients
{
    public enum PatientsTableMode
    {
        Default,
        SelectForVisit
    }

    public class PatiensListViewModel
    {
        public List<Patient> Patients = new();
        public string searchString = string.Empty;
        public PatientsTableMode mode = PatientsTableMode.Default;
    }
}
