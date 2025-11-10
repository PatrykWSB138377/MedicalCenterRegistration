// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Security.Claims;
using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class DoctorRatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DoctorRatingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET: DoctorRatings/Create
        [Authorize(Roles = Roles.Patient)]      
        public async Task<IActionResult> Create(int doctorId)
        {

            ModelState.Clear();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patient.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
            {
                return NotFound("Nie znaleziono pacjenta.");
            }
            bool hasVisited = await _context.Visit.AnyAsync(v => v.DoctorId == doctorId && v.PatientId == patient.Id && v.Status == Enums.Status.Finished);
            if (!hasVisited)
            {
                return BadRequest("Możesz ocenić tylko lekarzy, u których odbyła się wizyta.");
            }
            bool alreadyRated = await _context.DoctorRating.AnyAsync(r => r.DoctorId == doctorId && r.PatientId == patient.Id);
            if (alreadyRated)
            {
                var existingRating = await _context.DoctorRating
                    .FirstAsync(r => r.DoctorId == doctorId && r.PatientId == patient.Id);
                return RedirectToAction("Edit", new { id = existingRating.Id });
            }
            var rating = new MedicalCenterRegistration.Models.DoctorRating
            {
                DoctorId = doctorId,
                PatientId = patient.Id,                
            };
            return View(rating);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> Create([Bind("Id,Rating,Comment,DoctorId,PatientId,CreatedAt")] MedicalCenterRegistration.Models.DoctorRating doctorRating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patient.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null || patient.Id != doctorRating.PatientId)
            {
                return Unauthorized();
            }

            bool hasVisited = await _context.Visit.AnyAsync(v => v.DoctorId == doctorRating.DoctorId && v.PatientId == doctorRating.PatientId && v.Status == Enums.Status.Finished);
            if (!hasVisited)
            {
                return BadRequest("Możesz ocenić tylko lekarzy, u których odbyła się wizyta.");
            }

            bool alreadyRated = await _context.DoctorRating.AnyAsync(r => r.DoctorId == doctorRating.DoctorId && r.PatientId == doctorRating.PatientId);
            if (alreadyRated)
            {
                return BadRequest("Już oceniłeś tego lekarza.");
            }

            if (ModelState.IsValid)
            {
                doctorRating.CreatedAt = DateTime.Now;
                _context.Add(doctorRating);
                await _context.SaveChangesAsync();
                return RedirectToAction("List", "DoctorRatings");
            }
            return View(doctorRating);
        }


        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> Edit(int id)
        {
            var rating = await _context.DoctorRating.FindAsync(id);
            if (rating == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patient.FirstOrDefault(p => p.UserId == userId);
            if (patient == null || rating.PatientId != patient.Id)
                return Forbid();

            return View(rating);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Patient)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rating,Comment")] DoctorRating doctorRating)
        {
            var existing = await _context.DoctorRating.FindAsync(id);
            if (existing == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patient.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null || existing.PatientId != patient.Id)
                return Forbid();    

            existing.Rating = doctorRating.Rating;
            existing.Comment = doctorRating.Comment;
            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Doctors");
        }

        // GET: DoctorRatings/Delete/5
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.DoctorRating
                .Include(r => r.Doctor)
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // POST: DoctorRatings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.DoctorRating.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patient.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null || rating.PatientId != patient.Id)
                return Forbid();

            _context.DoctorRating.Remove(rating);
            await _context.SaveChangesAsync();  

            return RedirectToAction("List", "DoctorRatings");
        }
    }
}
