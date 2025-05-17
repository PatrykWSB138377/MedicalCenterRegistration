// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models
{
    public class DoctorSpecializationAssignment
    {
        public int Id { get; set; }
        public required int DoctorId { get; set; }
        public required Doctor Doctor { get; set; }

        public required int SpecializationId { get; set; }
        public required Specialization Specialization { get; set; }
    }
}
