// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models
{
    public class Visit
    {
        public int Id { get; set; }


        public required int DoctorId { get; set; }
        public required Doctor Doctor { get; set; }

        public required int PatientId { get; set; }
        public required Patient Patient { get; set; }


        public required int VisitScheduleId { get; set; }
        public required VisitSchedule VisitSchedule { get; set; }

        //public required Status Status { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
