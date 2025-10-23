// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Enums;

namespace MedicalCenterRegistration.Models
{
    public class Visit
    {
        public int Id { get; set; }


        public int DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }

        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }


        public int VisitScheduleId { get; set; }
        public virtual VisitSchedule VisitSchedule { get; set; }
        public virtual VisitSummary? VisitSummary { get; set; }

        public string VisitType { get; set; }

        public required Status Status { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
