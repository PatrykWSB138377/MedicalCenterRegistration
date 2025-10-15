// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Patient.Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        public async Task<IActionResult> Create()
        {
            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();

            var existingPatientData = await _context.Patient.FirstOrDefaultAsync(p => p.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (existingPatientData != null)
            {
                return RedirectToAction(nameof(Edit), new { id = existingPatientData.Id });
            }


            return View(existingPatientData);
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,LastName,DateOfBirth,Sex,PhoneNumber,PeselNumber,Street,HouseNumber,Province,District,PostalCode,City")] Patient patient)
        {
            patient.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            patient.User = await _context.Users.FindAsync(patient.UserId);
            patient.CreatedAt = DateTime.Now;


            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return RedirectToAction("Index", "Home");
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }


            var properties = patient.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(patient);
                ViewData[prop.Name] = value;
            }

            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,DateOfBirth,Sex,PhoneNumber,PeselNumber,Street,HouseNumber,Province,District,PostalCode,City")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            patient.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            patient.User = await _context.Users.FindAsync(patient.UserId);

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
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Edit));
            }

            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patient.FindAsync(id);
            if (patient != null)
            {
                _context.Patient.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patient.Any(e => e.Id == id);
        }
    }
}
