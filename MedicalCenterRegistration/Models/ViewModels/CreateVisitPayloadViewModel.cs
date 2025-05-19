// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.ViewModels
{
    public class CreateVisitPayloadViewModel
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }
}
