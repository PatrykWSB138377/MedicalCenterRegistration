// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using Microsoft.AspNetCore.Identity;

namespace MedicalCenterRegistration.Seeders
{
    public class ReceptionistsSeeder
    {
        public static async Task SeedReceptionistsAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // if already has receptionists, don't seed more
            if ((await userManager.GetUsersInRoleAsync(Roles.Receptionist)).Any())
            {
                return;
            }

            var passwordHasher = new PasswordHasher<IdentityUser>();
            var defaultPassword = "Recep123!";
            var passwordHash = passwordHasher.HashPassword(null, defaultPassword);

            var receptionistCounter = 1;

            var receptionistAccountFaker = new Bogus.Faker<IdentityUser>()
                 .RuleFor(u => u.UserName, f => $"recep{receptionistCounter++}@wp.pl")
                .RuleFor(u => u.Email, (f, u) => u.UserName)
                .RuleFor(u => u.EmailConfirmed, f => true);


            var fakeReceptionistAccounts = receptionistAccountFaker.Generate(3);


            var i = 1;
            foreach (var receptionist in fakeReceptionistAccounts)
            {
                var result = await userManager.CreateAsync(receptionist, defaultPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(receptionist, Roles.Receptionist);

                }
                else
                {
                    Console.WriteLine("********************************************************");
                    Console.WriteLine($"Failed to create user {receptionist.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                i++;
            }

            await context.SaveChangesAsync();
        }
    }
}
