// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.ViewModels.Visits
{
    public class AdminReceptionistVisitViewModel
    {
        public string Patient { get; set; }
        public string Doctor { get; set; }
        public DateOnly Date { get; set; }
        public string Time { get; set; }
        public string VisitType { get; set; }
        public string VisitStatus { get; set; }
    }
}
