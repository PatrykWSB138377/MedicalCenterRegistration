// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Helpers;

using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        // GET: Patients
        [Authorize(Roles = Roles.AdminAndReceptionist)]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // POST: GetPatients
        [Authorize(Roles = Roles.AdminAndReceptionist)]
        [HttpPost]
        public async Task<IActionResult> GetPatients()
        {
            var query = _context.Patient.Include(p => p.User).AsQueryable();

            var request = DataTableHelper.GetRequest(Request);

            var totalRecords = await query.CountAsync();

            // filtering
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var searchValue = request.SearchValue.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchValue.ToLower()) ||
                    p.LastName.ToLower().Contains(searchValue.ToLower()) ||
                    p.User.Email.ToLower().Contains(searchValue.ToLower())
                );
            }

            var filteredRecords = await query.CountAsync();

            // sorting
            query = query.ApplySorting(request);

            // paging
            var patients = await query
                .Skip(request.Start)
                .Take(request.Length)
                .ToListAsync();


            var patientsVm = new List<PatientInListViewModel>();
            foreach (var patient in patients)
            {
                patientsVm.Add(new PatientInListViewModel
                {
                    Id = patient.Id,
                    Name = patient.Name,
                    LastName = patient.LastName,
                    UserEmail = patient.User.Email,
                    CreatedAt = patient.CreatedAt.ToString("yyyy-MM-dd"),
                    mode = PatientsTableMode.Default
                });
            }


            var response = DataTableHelper.CreateResponse(request, totalRecords, filteredRecords, patientsVm);



            return Json(response);
        }


        // GET: Patients/Details/5
        [Authorize(Roles = Roles.AdminAndReceptionistAndPatient)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patient
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
                return NotFound();

            if (User.IsInRole("Patient"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (patient.UserId != userId)
                    return Forbid();
            }

            return View(patient);
        }

        [Authorize(Roles = Roles.ReceptionistAndPatient)]
        public async Task<IActionResult> Create(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();

            var existingPatient = await _context.Patient
                .FirstOrDefaultAsync(p => p.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (existingPatient != null)
            {
                return RedirectToAction(nameof(Edit), new { id = existingPatient.Id });
            }

            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [Authorize(Roles = Roles.Patient)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,LastName,DateOfBirth,Sex,PhoneNumber,PeselNumber,Street,HouseNumber,Province,District,PostalCode,City")] Patient patient,
            [FromForm] string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            patient.UserId = userId;
            patient.User = await _context.Users.FindAsync(userId);
            patient.CreatedAt = DateTime.Now;

            ModelState.Remove("returnUrl"); // don't validate returnUrl
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction(nameof(Edit), new { id = patient.Id });
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return View(patient);
        }

        // GET: Patients/Edit/5
        [Authorize(Roles = Roles.AdminAndReceptionistAndPatient)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patient.FindAsync(id);
            if (patient == null)
                return NotFound();


            if (User.IsInRole("Patient"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (patient.UserId != userId)
                    return Forbid();
            }

            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [Authorize(Roles = Roles.AdminAndReceptionistAndPatient)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,DateOfBirth,Sex,PhoneNumber,PeselNumber,Street,HouseNumber,Province,District,PostalCode,City")] Patient patient)
        {
            if (id != patient.Id)
                return NotFound();

            var existingPatient = await _context.Patient.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existingPatient == null)
                return NotFound();


            if (User.IsInRole("Patient"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existingPatient.UserId != userId)
                    return Forbid();
            }

            patient.UserId = existingPatient.UserId;
            patient.User = await _context.Users.FindAsync(existingPatient.UserId);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
                        return NotFound();
                    else
                        throw;
                }

                // Jeśli pacjent edytował swoje dane → wróć do szczegółów
                if (User.IsInRole("Patient"))
                    return RedirectToAction(nameof(Details), new { id = patient.Id });

                return RedirectToAction(nameof(Index));
            }

            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return View(patient);
        }

        // GET: Patients/Delete/5
        [Authorize(Roles = Roles.AdminAndReceptionistAndPatient)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patient
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
                return NotFound();


            if (User.IsInRole("Patient"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (patient.UserId != userId)
                    return Forbid();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = Roles.AdminAndReceptionistAndPatient)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patient.FindAsync(id);
            if (patient == null)
                return NotFound();


            if (User.IsInRole("Patient"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (patient.UserId != userId)
                    return Forbid();
            }

            _context.Patient.Remove(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patient.Any(e => e.Id == id);
        }

        // GET: Patients/CreateForUser
        [Authorize(Roles = Roles.AdminAndReceptionist)]
        public async Task<IActionResult> CreateForUser()
        {
            var usersInPatientRole = await _userManager.GetUsersInRoleAsync("Patient");
            var existingPatientUserIds = await _context.Patient.Select(p => p.UserId).ToListAsync();

            var availableUsers = usersInPatientRole
                .Where(u => !existingPatientUserIds.Contains(u.Id))
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.UserName}"
                })
                .ToList();

            var viewModel = new PatientCardViewModel
            {
                AvailableUsers = availableUsers
            };

            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return View(viewModel);
        }

        // POST: Patients/CreateForUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AdminAndReceptionist)]
        public async Task<IActionResult> CreateForUser(PatientCardViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var usersInPatientRole = await _userManager.GetUsersInRoleAsync("Patient");
                var existingPatientUserIds = await _context.Patient.Select(p => p.UserId).ToListAsync();

                vm.AvailableUsers = usersInPatientRole
                    .Where(u => !existingPatientUserIds.Contains(u.Id))
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = $"{u.UserName}"
                    })
                    .ToList();

                ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.SelectedUserId);
            if (user == null)
            {
                ModelState.AddModelError("SelectedUserId", "Wybrany użytkownik nie istnieje.");
                return View(vm);
            }

            var patient = new Patient
            {
                Name = vm.Name,
                LastName = vm.LastName,
                PhoneNumber = vm.PhoneNumber,
                PeselNumber = vm.PeselNumber,
                DateOfBirth = vm.DateOfBirth,
                Sex = vm.Sex,
                Street = vm.Street,
                HouseNumber = vm.HouseNumber,
                Province = vm.Province,
                District = vm.District,
                PostalCode = vm.PostalCode,
                City = vm.City,
                UserId = vm.SelectedUserId,
                User = user,
                CreatedAt = DateTime.Now
            };

            _context.Add(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }





    }
}
