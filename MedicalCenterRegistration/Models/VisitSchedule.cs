// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models
{
    public class VisitSchedule
    {
        public int Id { get; set; }

        public DateOnly VisitDate { get; set; }
        public TimeOnly VisitTimeStart { get; set; }
        public TimeOnly VisitTimeEnd { get; set; }
    }
}
