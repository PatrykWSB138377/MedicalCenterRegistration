// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;

namespace MedicalCenterRegistration.Seeders
{
    public class VisitsSeeder
    {
        public static async Task SeedVisitsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var visitReadyPatients = context.Patient
                .AsEnumerable()
                .Where((p, index) => index % 2 == 0) // select every second patient
                .ToList();

            var allDoctors = context.Doctor.ToList();


            var existingPendingVisits = context.Visit
                .Where(v => v.Status == Status.Pending)
                .ToList();


            foreach (var patient in visitReadyPatients)
            {

                if (existingPendingVisits.Any())
                {
                    break;
                }

                var random = new Random();

                var randomDoctor = allDoctors[random.Next(allDoctors.Count)];
                var visitDate = DateOnly.FromDateTime(DateTime.Now.AddDays(random.Next(1, 30))); //visits scheduled within the next 30 days
                var visitTime = new TimeOnly(random.Next(8, 16), 0);

                // check if a visit already exists for this patient with the same doctor on the same date
                bool visitExists = context.Visit.Any(v =>
                    v.PatientId == patient.Id &&
                    v.DoctorId == randomDoctor.Id &&
                    v.VisitSchedule.VisitDate == visitDate &&
                    v.VisitSchedule.VisitTimeStart == visitTime);

                if (!visitExists)
                {

                    var visitSchedule = new VisitSchedule
                    {
                        VisitDate = visitDate,
                        VisitTimeStart = visitTime,
                        VisitTimeEnd = visitTime.AddMinutes(30)
                    };

                    context.VisitSchedule.Add(visitSchedule);
                    await context.SaveChangesAsync();

                    var visitFaker = new Bogus.Faker<Visit>()
                        .RuleFor(v => v.PatientId, patient.Id)
                        .RuleFor(v => v.DoctorId, randomDoctor.Id)
                        .RuleFor(v => v.VisitType, "Wizyta kontrolna")
                        .RuleFor(v => v.Status, Status.Pending)
                        .RuleFor(v => v.VisitScheduleId, visitSchedule.Id)
                        .RuleFor(v => v.CreatedAt, f => f.Date.Recent())
                        .RuleFor(v => v.UpdatedAt, (f, v) => v.CreatedAt);

                    var visit = visitFaker.Generate();

                    context.Visit.Add(visit);
                }

            }


            // generate some already completed visits
            var completedVisitsPatients = context.Patient
                .AsEnumerable()
                .Where((p, index) => index % 3 == 0) // select every third patient
                .ToList();

            var existingCompletedVisits = context.Visit
                .Where(v => v.Status == Status.Finished)
                .ToList();

            foreach (var patient in completedVisitsPatients)
            {

                if (existingCompletedVisits.Any())
                {
                    break;
                }

                var random = new Random();
                var randomDoctor = allDoctors[random.Next(allDoctors.Count)];

                var pastVisitDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(1, 30))); // visits scheduled within the last 30 days
                var pastVisitTime = new TimeOnly(random.Next(8, 16), 0);

                // check if a visit already exists for this patient with the same doctor on the same date
                bool visitExists = context.Visit.Any(v =>
                    v.PatientId == patient.Id &&
                    v.DoctorId == randomDoctor.Id &&
                    v.VisitSchedule.VisitDate == pastVisitDate &&
                    v.VisitSchedule.VisitTimeStart == pastVisitTime);

                if (!visitExists)
                {

                    var visitSchedule = new VisitSchedule
                    {
                        VisitDate = pastVisitDate,
                        VisitTimeStart = pastVisitTime,
                        VisitTimeEnd = pastVisitTime.AddMinutes(30)
                    };

                    context.VisitSchedule.Add(visitSchedule);
                    await context.SaveChangesAsync();

                    var visitFaker = new Bogus.Faker<Visit>()
                        .RuleFor(v => v.PatientId, patient.Id)
                        .RuleFor(v => v.DoctorId, randomDoctor.Id)
                        .RuleFor(v => v.VisitType, "Wizyta kontrolna")
                        .RuleFor(v => v.Status, Status.Finished)
                        .RuleFor(v => v.VisitScheduleId, visitSchedule.Id)
                        .RuleFor(v => v.CreatedAt, f => f.Date.Recent(60))
                        .RuleFor(v => v.UpdatedAt, (f, v) => v.CreatedAt);

                    var visit = visitFaker.Generate();

                    context.Visit.Add(visit);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
