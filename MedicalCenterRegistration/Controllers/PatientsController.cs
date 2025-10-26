// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private async Task<List<dynamic>> GetAvailablePatientsAsync()
        {
            var usersWithPatients = _context.Patient.Select(p => p.UserId).ToList();
            var result = new List<dynamic>();
            var allUsers = await _userManager.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Patient") && !usersWithPatients.Contains(user.Id))
                {
                    result.Add(new { user.Id, user.Email });
                }
            }
            return result;
        }


        // GET: Patients
        [Authorize(Roles = "Admin, Receptionist")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Patient.Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Patients/Details/5
        [Authorize(Roles = "Patient, Receptionist, Admin")]
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

        [Authorize(Roles = "Patient, Receptionist")]
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
            return RedirectToAction("Index", "Home");
        }

        // GET: Patients/Edit/5
        [Authorize(Roles = "Patient, Receptionist, Admin")]
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
        [Authorize(Roles = "Patient, Receptionist, Admin")]
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
        [Authorize(Roles = "Patient, Receptionist, Admin")]
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
        [Authorize(Roles = "Patient, Receptionist, Admin")]
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

        [Authorize(Roles = "Receptionist,Admin")]
        public async Task<IActionResult> Assign(string? userId)
        {
            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();

            var usersWithPatients = _context.Patient.Select(p => p.UserId).ToList();

            var allUsers = await _context.Users.ToListAsync();
            var userList = new List<dynamic>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Patient") && !usersWithPatients.Contains(user.Id))
                {
                    userList.Add(new { user.Id, user.Email });
                }
            }

            if (userId == null)
            {
                ViewData["Users"] = userList;
                return View(new PatientCardViewModel());
            }

            var existingPatient = await _context.Patient
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingPatient == null)
            {
                var user = await _context.Users.FindAsync(userId);
                var model = new PatientCardViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    ExistsAlready = false
                };
                return View(model);
            }
            else
            {
                var model = new PatientCardViewModel
                {
                    UserId = existingPatient.UserId,
                    Email = existingPatient.User.Email,
                    Name = existingPatient.Name,
                    LastName = existingPatient.LastName,
                    DateOfBirth = existingPatient.DateOfBirth,
                    Sex = existingPatient.Sex,
                    PhoneNumber = existingPatient.PhoneNumber,
                    PeselNumber = existingPatient.PeselNumber,
                    Street = existingPatient.Street,
                    HouseNumber = existingPatient.HouseNumber,
                    Province = existingPatient.Province,
                    District = existingPatient.District,
                    PostalCode = existingPatient.PostalCode,
                    City = existingPatient.City,
                    ExistsAlready = true
                };
                return View(model);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Receptionist,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(PatientCardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
                ViewData["Users"] = await GetAvailablePatientsAsync();
                return View(model);
            }

            var existingPatient = await _context.Patient.FirstOrDefaultAsync(p => p.UserId == model.UserId);
            if (existingPatient == null)
            {
                var patient = new Patient
                {
                    UserId = model.UserId,
                    Name = model.Name,
                    LastName = model.LastName,
                    DateOfBirth = (DateTime)model.DateOfBirth,
                    Sex = model.Sex,
                    PhoneNumber = model.PhoneNumber,
                    PeselNumber = model.PeselNumber,
                    Street = model.Street,
                    HouseNumber = model.HouseNumber,
                    Province = model.Province,
                    District = model.District,
                    PostalCode = model.PostalCode,
                    City = model.City,
                    CreatedAt = DateTime.Now
                };
                _context.Patient.Add(patient);
            }
            else
            {
                existingPatient.Name = model.Name;
                existingPatient.LastName = model.LastName;
                existingPatient.DateOfBirth = (DateTime)model.DateOfBirth;
                existingPatient.Sex = model.Sex;
                existingPatient.PhoneNumber = model.PhoneNumber;
                existingPatient.PeselNumber = model.PeselNumber;
                existingPatient.Street = model.Street;
                existingPatient.HouseNumber = model.HouseNumber;
                existingPatient.Province = model.Province;
                existingPatient.District = model.District;
                existingPatient.PostalCode = model.PostalCode;
                existingPatient.City = model.City;
                _context.Patient.Update(existingPatient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
