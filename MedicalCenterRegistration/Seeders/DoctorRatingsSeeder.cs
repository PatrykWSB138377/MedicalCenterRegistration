// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;

namespace MedicalCenterRegistration.Seeders
{
    public class DoctorRatingsSeeder
    {
        public static async Task SeedDoctorRatingsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var finishedVisits = context.Visit
                .Where(v => v.Status == Status.Finished)
                .ToList();


            var existingRatings = context.DoctorRating.ToList();
            if (existingRatings.Any())
            {
                return;
            }

            foreach (var visit in finishedVisits)
            {

                var doctorOfVisit = visit.Doctor;
                var patientOfVisit = visit.Patient;

                var ratingOfPatientForDoctorExists = context.DoctorRating
                    .Any(dr => dr.DoctorId == doctorOfVisit.Id && dr.PatientId == patientOfVisit.Id);

                if (ratingOfPatientForDoctorExists)
                {
                    continue;
                }


                var ratingFaker = new Bogus.Faker<DoctorRating>()
                    .RuleFor(dr => dr.Rating, f => f.Random.Int(3, 5))
                    .RuleFor(dr => dr.Comment, f => f.Lorem.Sentence(f.Random.Int(5, 20)))
                    .RuleFor(dr => dr.CreatedAt, f => f.Date.Past(1))
                    .RuleFor(dr => dr.DoctorId, doctorOfVisit.Id)
                    .RuleFor(dr => dr.PatientId, patientOfVisit.Id);

                var doctorRating = ratingFaker.Generate();

                context.DoctorRating.Add(doctorRating);
            }

            context.SaveChanges();
        }
    }
}
