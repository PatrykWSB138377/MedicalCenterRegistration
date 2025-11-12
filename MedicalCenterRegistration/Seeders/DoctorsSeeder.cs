// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;

namespace MedicalCenterRegistration.Seeders
{
    public class DoctorsSeeder
    {
        public static async Task SeedDoctorsAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();


            var specializations = new[]
            {
                "Kardiolog",
                "Dermatolog",
                "Neurolog",
                "Urolog",
                "Ginekolog",
                "Okulista",
                "Laryngolog"
            };


            // seed specializations
            foreach (var specialization in specializations)
            {
                if (!context.Specialization.Any(s => s.Name == specialization))
                {
                    context.Specialization.Add(new Specialization
                    {
                        Name = specialization
                    });
                }
            }
            await context.SaveChangesAsync();

            // seed images
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "doctors");
            var fileProvider = new FileExtensionContentTypeProvider();

            foreach (var filePath in Directory.GetFiles(imagesPath).Take(specializations.Length))
            {
                var fileName = Path.GetFileName(filePath);
                if (!context.PublicImage.Any(pi => pi.FileName == fileName))
                {
                    fileProvider.TryGetContentType(fileName, out string contentType);

                    var image = new PublicImage
                    {
                        FileName = fileName,
                        ContentType = contentType ?? "application/octet-stream"
                    };

                    context.PublicImage.Add(image);
                }
            }
            await context.SaveChangesAsync();


            // if already has doctors, don't seed more
            if ((await userManager.GetUsersInRoleAsync(Roles.Doctor)).Any())
            {
                return;
            }

            var passwordHasher = new PasswordHasher<IdentityUser>();
            var defaultPassword = "Doctor123!";
            var passwordHash = passwordHasher.HashPassword(null, defaultPassword);


            var doctorCounter = 1;

            var doctorAccountFaker = new Bogus.Faker<IdentityUser>()
                .RuleFor(u => u.UserName, f => $"doctor{doctorCounter++}@wp.pl")
                .RuleFor(u => u.Email, (f, u) => u.UserName)
                .RuleFor(u => u.EmailConfirmed, f => true);


            var fakeDoctorAccounts = doctorAccountFaker.Generate(specializations.Length);


            var i = 1;
            foreach (var doctor in fakeDoctorAccounts)
            {
                var result = await userManager.CreateAsync(doctor, defaultPassword);

                if (result.Succeeded)
                {

                    await userManager.AddToRoleAsync(doctor, Roles.Doctor);

                    var publicImage = context.PublicImage.Skip(i - 1).First();
                    var doctorFaker = new Bogus.Faker<Doctor>()
                        .RuleFor(d => d.Name, f => f.Name.FirstName())
                        .RuleFor(d => d.LastName, f => f.Name.LastName())
                        .RuleFor(d => d.Description, f => f.Lorem.Sentence(10))
                        .RuleFor(d => d.DateOfBirth, f => f.Date.Past(50, DateTime.Now.AddYears(-30)))
                        .RuleFor(d => d.Sex, f => f.PickRandom(new[] { Sex.Male, Sex.Female, Sex.Other }))
                        .RuleFor(d => d.UserId, f => doctor.Id)
                        .RuleFor(d => d.CreatedAt, f => f.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now))
                        .RuleFor(d => d.ImageId, f => publicImage.Id);


                    var doctorEntity = doctorFaker.Generate(1)[0];
                    context.Doctor.Add(doctorEntity);
                    await context.SaveChangesAsync();

                    // link specialization to doctor
                    var specialization = context.Specialization.First(s => s.Name == specializations[i - 1]);
                    context.DoctorSpecialization.Add(new DoctorSpecialization
                    {
                        DoctorId = doctorEntity.Id,
                        SpecializationId = specialization.Id
                    });
                }
                else
                {
                    Console.WriteLine("********************************************************");
                    Console.WriteLine($"Failed to create doctor {doctor.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                i++;
            }





            await context.SaveChangesAsync();
        }
    }
}
