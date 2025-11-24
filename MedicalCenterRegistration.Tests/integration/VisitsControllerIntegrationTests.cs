// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Controllers;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels.Visits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedicalCenterRegistration.Tests.Integration
{
    public class VisitsControllerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly VisitsController _controller;
        private readonly PatientService _patientService;
        private readonly Mock<ILogger<VisitsController>> _loggerMock;
        private readonly UserManager<IdentityUser> _userManager;

        public VisitsControllerIntegrationTests()
        {
            // in-memory database setup
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // mocks setup
            _loggerMock = new Mock<ILogger<VisitsController>>();
            
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null).Object;

            // patient service setup
            _patientService = new PatientService(_context, _userManager);

            // controller setup
            _controller = new VisitsController(_context, _patientService, _loggerMock.Object);

            // temp data setup
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            SeedDatabase();
        }

        // initial data seeding before each test
        private void SeedDatabase()
        {
            // create public image for doctor
            var doctorImage = new PublicImage
            {
                Id = 1,
                FileName = "doctor.jpg",
                ContentType = "image/jpeg"
            };
            _context.PublicImage.Add(doctorImage);

            // create patient
            var patient = new Patient
            {
                Id = 1,
                Name = "Jan",
                LastName = "Kowalski",
                PhoneNumber = "123456789",
                PeselNumber = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                Sex = Sex.Male,
                Street = "Main St",
                HouseNumber = "1",
                Province = "Mazowieckie",
                District = "Warszawa",
                PostalCode = "00-001",
                City = "Warszawa",
                UserId = "patient-user-1",
                CreatedAt = DateTime.Now
            };
            _context.Patient.Add(patient);

            // create doctor
            var doctor = new Doctor
            {
                Id = 1,
                Name = "Anna",
                LastName = "Nowak",
                Description = "Experienced doctor",
                DateOfBirth = new DateTime(1980, 1, 1),
                Sex = Sex.Female,
                ImageId = 1,
                UserId = "doctor-user-1",
                CreatedAt = DateTime.Now
            };
            _context.Doctor.Add(doctor);

            // create specialization
            var specialization = new Specialization
            {
                Id = 1,
                Name = "Cardiology"
            };
            _context.Specialization.Add(specialization);

            // create doctor specialization
            var doctorSpecialization = new DoctorSpecialization
            {
                DoctorId = 1,
                SpecializationId = 1
            };
            _context.DoctorSpecialization.Add(doctorSpecialization);

            // create visit schedule
            var visitSchedule = new VisitSchedule
            {
                Id = 1,
                VisitDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                VisitTimeStart = new TimeOnly(10, 0),
                VisitTimeEnd = new TimeOnly(10, 30)
            };
            _context.VisitSchedule.Add(visitSchedule);

            // create visit
            var visit = new Visit
            {
                Id = 1,
                DoctorId = 1,
                PatientId = 1,
                VisitScheduleId = 1,
                VisitType = "Wizyta kontrolna",
                Status = Status.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Visit.Add(visit);

            _context.SaveChanges();
        }

        // helper method to setup controller's HttpContext with defined user
        private void SetupControllerWithUser(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Index_AsPatient_ReturnsViewWithPatientVisits()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            // act
            var result = await _controller.Index();

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Visit>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(1, model.First().PatientId);
        }

        [Fact]
        public async Task AllVisits_AsReceptionist_ReturnsViewWithAllVisits()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);

            // act
            var result = await _controller.AllVisits();

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Visit>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task DoctorVisits_AsDoctor_ReturnsViewWithDoctorVisits()
        {
            // arrange
            SetupControllerWithUser("doctor-user-1", Roles.Doctor);

            // act
            var result = await _controller.DoctorVisits();

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Visit>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(1, model.First().DoctorId);
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFound()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            // act
            var result = await _controller.Delete(null);

            // assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithNonExistentId_ReturnsNotFound()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            // act
            var result = await _controller.Delete(999);

            // assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_DeletesVisitAndRedirects()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);
            var initialVisitCount = _context.Visit.Count();

            // act
            var result = await _controller.DeleteConfirmed(1);

            // assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(initialVisitCount - 1, _context.Visit.Count());
            Assert.Null(_context.Visit.Find(1));
        }

        [Fact]
        public async Task DeleteConfirmed_WithNonExistentId_RedirectsWithoutError()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);
            var initialVisitCount = _context.Visit.Count();

            // act
            var result = await _controller.DeleteConfirmed(999);

            // assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(initialVisitCount, _context.Visit.Count());
        }

        [Fact]
        public async Task ChoosePatient_AsReceptionist_ReturnsView()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);

            // act
            var result = await _controller.ChoosePatient();

            // assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewWithVisit()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);

            // act
            var result = await _controller.Edit(1);

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["DoctorId"]);
            Assert.NotNull(viewResult.ViewData["PatientId"]);
        }

        [Fact]
        public async Task Edit_WithNullId_ReturnsNotFound()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);

            // act
            var result = await _controller.Edit(null);

            // assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithNonExistentId_ReturnsNotFound()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);

            // act
            var result = await _controller.Edit(999);

            // assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SubmitSpecializationType_WithValidData_SetsTempDataAndRedirects()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            // act
            var result = await _controller.SubmitSpecializationType("1", "2");

            // assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.Equal("1", _controller.TempData["SpecializationId"]);
            Assert.Equal("2", _controller.TempData["PatientId"]);
        }

        [Fact]
        public async Task SubmitSpecializationType_WithEmptySpecializationId_ReturnsViewWithModelError()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            // act
            var result = await _controller.SubmitSpecializationType("", null);

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ChooseSpecializationType", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Get_WithoutSpecializationId_RedirectsToChooseSpecializationType()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            // act
            var result = await _controller.Create();

            // assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ChooseSpecializationType", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Get_AsPatient_WithSpecializationId_ReturnsViewWithDoctors()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);
            _controller.TempData["SpecializationId"] = "1";

            // act
            var result = await _controller.Create();

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateVisitCreationViewModel>(viewResult.Model);
            Assert.Single(model.Doctors);
            Assert.Equal("Jan Kowalski", $"{model.Patient.Name} {model.Patient.LastName}");
        }

        [Fact]
        public async Task Create_Get_AsReceptionist_WithPatientId_ReturnsViewWithDoctors()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);
            _controller.TempData["SpecializationId"] = "1";
            _controller.TempData["PatientId"] = "1";

            // act
            var result = await _controller.Create();

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateVisitCreationViewModel>(viewResult.Model);
            Assert.Single(model.Doctors);
            Assert.NotNull(model.Patient);
            Assert.Equal("Jan", model.Patient.Name);
        }

        [Fact]
        public async Task Create_Get_AsReceptionist_WithoutPatientId_ReturnsBadRequest()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);
            _controller.TempData["SpecializationId"] = "1";

            // act
            var result = await _controller.Create();

            // assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_Post_WithValidData_CreatesVisitAndRedirectsToIndex()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            var payload = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 1,
                Date = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd"),
                Time = "14:00"
            };

            var initialVisitCount = _context.Visit.Count();

            // act
            var result = await _controller.Create(payload);

            // assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(initialVisitCount + 1, _context.Visit.Count());

            var newVisit = _context.Visit.OrderByDescending(v => v.Id).First();
            Assert.Equal(1, newVisit.DoctorId);
            Assert.Equal(1, newVisit.PatientId);
            Assert.Equal(Status.Pending, newVisit.Status);
        }

        [Fact]
        public async Task Create_Post_AsReceptionist_RedirectsToAllVisits()
        {
            // arrange
            SetupControllerWithUser("receptionist-user-1", Roles.Receptionist);

            var payload = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 1,
                Date = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd"),
                Time = "15:00"
            };

            // act
            var result = await _controller.Create(payload);

            // assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AllVisits", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_WithInvalidPatient_ReturnsViewWithError()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            var payload = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 999,
                Date = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd"),
                Time = "14:00"
            };

            // act
            var result = await _controller.Create(payload);

            // assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Post_CreatesVisitSchedule()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            var initialScheduleCount = _context.VisitSchedule.Count();

            var payload = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 1,
                Date = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd"),
                Time = "16:30"
            };

            // act
            await _controller.Create(payload);

            // assert
            Assert.Equal(initialScheduleCount + 1, _context.VisitSchedule.Count());

            var newSchedule = _context.VisitSchedule.OrderByDescending(s => s.Id).First();
            Assert.Equal(new TimeOnly(16, 30), newSchedule.VisitTimeStart);
            Assert.Equal(new TimeOnly(17, 0), newSchedule.VisitTimeEnd);
        }

        [Fact]
        public async Task Create_Post_SetsCorrectVisitProperties()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            var visitDate = DateTime.Now.AddDays(20);
            var payload = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 1,
                Date = visitDate.ToString("yyyy-MM-dd"),
                Time = "09:00"
            };

            // act
            await _controller.Create(payload);

            // assert
            var newVisit = _context.Visit.OrderByDescending(v => v.Id).First();
            Assert.Equal("Wizyta kontrolna", newVisit.VisitType);
            Assert.Equal(Status.Pending, newVisit.Status);
            Assert.NotEqual(default(DateTime), newVisit.CreatedAt);
            Assert.True((DateTime.Now - newVisit.CreatedAt).TotalSeconds < 5);
        }

        [Fact]
        public async Task Create_Post_WithMultipleVisits_CreatesAllSuccessfully()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            var initialCount = _context.Visit.Count();

            // act - create first visit
            var payload1 = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 1,
                Date = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"),
                Time = "10:00"
            };
            await _controller.Create(payload1);

            // act - create second visit
            var payload2 = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 1,
                Date = DateTime.Now.AddDays(6).ToString("yyyy-MM-dd"),
                Time = "11:00"
            };
            await _controller.Create(payload2);

            // assert
            Assert.Equal(initialCount + 2, _context.Visit.Count());
        }

        [Fact]
        public async Task Create_Get_AsPatient_WithoutPatientData_RedirectsToCreatePatient()
        {
            // arrange
            SetupControllerWithUser("patient-user-without-data", Roles.Patient);
            _controller.TempData["SpecializationId"] = "1";

            // act
            var result = await _controller.Create();

            // assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.Equal("Patients", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Create_Post_AsPatient_VerifyDoctor()
        {
            // arrange
            SetupControllerWithUser("patient-user-1", Roles.Patient);

            var payload = new CreateVisitPayloadViewModel
            {
                DoctorId = 1,
                PatientId = 1,
                Date = DateTime.Now.AddDays(8).ToString("yyyy-MM-dd"),
                Time = "13:00"
            };

            // act
            await _controller.Create(payload);

            // assert
            var newVisit = _context.Visit
                .Include(v => v.Doctor)
                .OrderByDescending(v => v.Id)
                .First();

            Assert.NotNull(newVisit.Doctor);
            Assert.Equal("Anna", newVisit.Doctor.Name);
            Assert.Equal("Nowak", newVisit.Doctor.LastName);
        }

        // cleans up the in-memory database after each test
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
