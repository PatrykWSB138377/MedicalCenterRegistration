// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => { options.SignIn.RequireConfirmedAccount = false; })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PatientService>();

// Admin ENV variables
string adminEmail = builder.Configuration["AdminCredentials:Email"];
string adminPassword = builder.Configuration["AdminCredentials:Password"];

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    await RolesSeeder.SeedRolesAsync(services);
    await AdminSeeder.SeedAdminAsync(services);
    await ReceptionistsSeeder.SeedReceptionistsAsync(services);
    await PatientsSeeder.SeedPatientsnAsync(services);
    await DoctorsSeeder.SeedDoctorsAsync(services);
    await VisitsSeeder.SeedVisitsAsync(services);
    await DoctorRatingsSeeder.SeedDoctorRatingsAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

string publicImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData", "PublicImages");

// this is needed because azure executes from a different directory and throws errors otherwise
if (!Directory.Exists(publicImagesPath))
{
    Directory.CreateDirectory(publicImagesPath);
}

var provider1 = new PhysicalFileProvider(publicImagesPath);

var provider2 = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "doctors"));

var compositeProvider = new CompositeFileProvider(provider1, provider2);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = compositeProvider,
    RequestPath = "/public-images"
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
