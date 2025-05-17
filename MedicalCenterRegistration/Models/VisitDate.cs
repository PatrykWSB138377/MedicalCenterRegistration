// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models
{
    public class VisitSchedule
    {
        public int Id { get; set; }

        public required DateOnly VisitDate { get; set; }
        public required TimeOnly VisitTimeStart { get; set; }
        public required TimeOnly VisitTimeEnd { get; set; }

        public required int VisitId { get; set; }
        public required Visit Visit { get; set; }
    }
}
