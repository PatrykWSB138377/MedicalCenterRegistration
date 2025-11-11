// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Identity;

namespace MedicalCenterRegistration.Seeders
{
    public class PatientsSeeder
    {
        public static async Task SeedPatientsnAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();


            var provinceDistricts = new Dictionary<string, string[]>
            {
                ["Wielkopolskie"] = new[] { "Poznań", "Kalisz", "Konin" },
                ["Mazowieckie"] = new[] { "Warszawa", "Radom", "Płock" },
                ["Dolnośląskie"] = new[] { "Wrocław", "Legnica", "Wałbrzych" },
                ["Opolskie"] = new[] { "Opole", "Kędzierzyn-Koźle", "Nysa" },
                ["Śląskie"] = new[] { "Katowice", "Gliwice", "Częstochowa" },
                ["Łódzkie"] = new[] { "Łódź", "Piotrków Trybunalski", "Skierniewice" },
                ["Kujawsko-pomorskie"] = new[] { "Bydgoszcz", "Toruń", "Włocławek" },
                ["Lubuskie"] = new[] { "Gorzów Wielkopolski", "Zielona Góra" },
                ["Pomorskie"] = new[] { "Gdańsk", "Gdynia", "Sopot" },
                ["Zachodnio-Pomorskie"] = new[] { "Szczecin", "Koszalin", "Stargard" }
            };

            // if already has patients, don't seed more
            if ((await userManager.GetUsersInRoleAsync(Roles.Patient)).Any())
            {
                return;
            }

            var passwordHasher = new PasswordHasher<IdentityUser>();
            var defaultPassword = "Patient123!";
            var passwordHash = passwordHasher.HashPassword(null, defaultPassword);

            var patientAccountFaker = new Bogus.Faker<IdentityUser>()
                .RuleFor(u => u.UserName, f => f.Internet.Email())
                .RuleFor(u => u.Email, (f, u) => u.UserName)
                .RuleFor(u => u.EmailConfirmed, f => true);


            var fakePatientAccounts = patientAccountFaker.Generate(100);


            var i = 1;
            foreach (var patient in fakePatientAccounts)
            {
                var result = await userManager.CreateAsync(patient, defaultPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patient, Roles.Patient);


                    // create patient card for every 2nd patient
                    if (i % 2 == 0)
                    {
                        var patientCardFaker = new Bogus.Faker<Patient>()
                            .RuleFor(pc => pc.Name, f => f.Name.FirstName())
                            .RuleFor(pc => pc.LastName, f => f.Name.LastName())
                            .RuleFor(pc => pc.DateOfBirth, f => f.Date.Past(80, DateTime.Now.AddYears(-18)))
                            .RuleFor(pc => pc.City, f => f.Address.City())
                            .RuleFor(pc => pc.Street, f => f.Address.StreetAddress())
                            .RuleFor(pc => pc.HouseNumber, f => f.Address.BuildingNumber())
                            .RuleFor(pc => pc.Province, f => f.PickRandom("Wielkopolskie", "Mazowieckie", "Dolnośląskie", "Opolskie", "Śląskie", "Łódzkie", "Kujawsko-pomorskie", "Lubuskie", "Pomorskie", "Zachodnio-Pomorskie"))
                            .RuleFor(pc => pc.District, (f, pc) => f.PickRandom(provinceDistricts[pc.Province]))
                            .RuleFor(pc => pc.PostalCode, f => f.Address.ZipCode())
                            .RuleFor(pc => pc.PeselNumber, f => f.Random.Replace("###########"))
                            .RuleFor(pc => pc.PhoneNumber, f => f.Phone.PhoneNumber())
                            .RuleFor(pc => pc.Sex, f => f.PickRandom(new[] { Sex.Male, Sex.Female, Sex.Other }))
                            .RuleFor(pc => pc.UserId, f => patient.Id) // link to IdentityUser
                            .RuleFor(pc => pc.CreatedAt, f => f.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now));

                        var patientCard = patientCardFaker.Generate();

                        context.Patient.Add(patientCard);
                    }
                }
                else
                {
                    Console.WriteLine("********************************************************");
                    Console.WriteLine($"Failed to create user {patient.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                i++;
            }

            await context.SaveChangesAsync();
        }
    }
}
