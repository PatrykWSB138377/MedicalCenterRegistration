// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class VisitsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PatientService _patientService;

        public VisitsController(ApplicationDbContext context, PatientService patientService)
        {
            _context = context;
            _patientService = patientService;
        }

        // GET: Visits
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Visit.Include(v => v.Doctor).Include(v => v.Patient).Include(v => v.VisitSchedule);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: ChooseVisitType
        [Authorize]
        public async Task<IActionResult> ChooseVisitType()
        {
            var hasPatientInfo = await _patientService.HasPatientEntryAsync(User);
            if (!hasPatientInfo)
            {
                return RedirectToAction("Create", "Patients", new { returnUrl = Url.Action(nameof(ChooseVisitType)) });
            }

            return View();
        }

        // POST: Visits
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseVisitType(string visitType)
        {

            if (string.IsNullOrEmpty(visitType))
            {
                ModelState.AddModelError("", "Please select a visit type.");
                return View();
            }

            TempData["VisitType"] = visitType;

            return RedirectToAction(nameof(Create));
        }



        // GET: Visits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await _context.Visit
                .Include(v => v.Doctor)
                .Include(v => v.Patient)
                .Include(v => v.VisitSchedule)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }

        // GET: Visits/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            if (TempData["VisitType"] == null)
            {
                return RedirectToAction(nameof(ChooseVisitType));
            }

            TempData.Keep("VisitType");

            var patient = await _patientService.GetPatientByUserIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (patient == null)
            {
                ModelState.AddModelError("", "Patient not found.");
                return RedirectToAction("Create", "Patients");
            }

            var doctors = await _context.Doctor.Include(d => d.Image).ToListAsync();
            var doctorIds = doctors.Select(d => d.Id).ToList();

            var visits = await _context.Visit
              .Where(v => doctorIds.Contains(v.DoctorId))
              .Include(v => v.VisitSchedule)
              .ToListAsync();

            var doctorScheduledVisits = visits
                .GroupBy(v => v.DoctorId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(v => v.VisitSchedule).ToList()
                );

            var viewModel = new CreateVisitCreationViewModel
            {
                Doctors = doctors,
                Patient = patient,
                DoctorScheduledVisits = doctorScheduledVisits
            };

            return View(viewModel);
        }

        // POST: Visits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorId, PatientId, Date, Time")] CreateVisitPayloadViewModel visitData)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(visitData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            if (TempData["VisitType"] == null)
            {
                return RedirectToAction(nameof(ChooseVisitType));
            }

            DateOnly visitDate = DateOnly.ParseExact(visitData.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            TimeOnly visitTimeStart = TimeOnly.ParseExact(visitData.Time, "HH:mm", CultureInfo.InvariantCulture);
            TimeOnly visitTimeEnd = visitTimeStart.AddMinutes(30);
            var visitSchedule = new VisitSchedule
            {
                VisitDate = visitDate,
                VisitTimeStart = visitTimeStart,
                VisitTimeEnd = visitTimeEnd
            };


            var isScheduleValid = Validator.TryValidateObject(visitSchedule, new ValidationContext(visitSchedule), null, true);

            if (!isScheduleValid)
            {
                ModelState.AddModelError("", "Invalid schedule data.");
                return View();
            }

            // create schhedule
            _context.VisitSchedule.Add(visitSchedule);
            await _context.SaveChangesAsync();

            var patient = await _patientService.GetPatientByUserIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (patient == null)
            {
                ModelState.AddModelError("", "Patient not found.");
                return View();
            }

            var visit = new Visit
            {
                DoctorId = visitData.DoctorId,
                Doctor = await _context.Doctor.FindAsync(visitData.DoctorId),
                PatientId = patient.Id,
                Patient = patient,
                VisitScheduleId = visitSchedule.Id,
                VisitType = TempData["VisitType"]?.ToString(),
                CreatedAt = DateTime.Now,
            };


            var isVisitValid = Validator.TryValidateObject(visit, new ValidationContext(visit), null, true);
            if (!isVisitValid)
            {
                ModelState.AddModelError("", "Invalid visit data.");
                return View();
            }


            _context.Visit.Add(visit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Visits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await _context.Visit.FindAsync(id);
            if (visit == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName", visit.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "LastName", visit.PatientId);
            ViewData["VisitScheduleId"] = new SelectList(_context.Set<VisitSchedule>(), "Id", "Id", visit.VisitScheduleId);
            return View(visit);
        }

        // POST: Visits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DoctorId,PatientId,VisitScheduleId,CreatedAt")] Visit visit)
        {
            if (id != visit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(visit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitExists(visit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName", visit.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "LastName", visit.PatientId);
            ViewData["VisitScheduleId"] = new SelectList(_context.Set<VisitSchedule>(), "Id", "Id", visit.VisitScheduleId);
            return View(visit);
        }

        // GET: Visits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visit = await _context.Visit
                .Include(v => v.Doctor)
                .Include(v => v.Patient)
                .Include(v => v.VisitSchedule)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }

        // POST: Visits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visit = await _context.Visit.FindAsync(id);
            if (visit != null)
            {
                _context.Visit.Remove(visit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VisitExists(int id)
        {
            return _context.Visit.Any(e => e.Id == id);
        }
    }
}
