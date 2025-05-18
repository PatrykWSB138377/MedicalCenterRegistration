// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class VisitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Visits
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Visit.Include(v => v.Doctor).Include(v => v.Patient).Include(v => v.VisitSchedule);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: ChooseVisitType
        public async Task<IActionResult> ChooseVisitType()
        {
            return View();
        }

        // POST: Visits
        [HttpPost]
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
        public IActionResult Create()
        {
            if (TempData["VisitType"] == null)
            {
                return RedirectToAction(nameof(ChooseVisitType));
            }

            TempData.Keep("VisitType");
            ViewData["PatientId"] = 1; //TODO: add actual patient id

            //ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName");
            //ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "LastName");
            //ViewData["VisitScheduleId"] = new SelectList(_context.Set<VisitSchedule>(), "Id", "Id");
            return View();
        }

        // POST: Visits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorId, PatientId, Date, Time")] CreateVisitViewModel visitData)
        {
            if (TempData["VisitType"] == null)
            {
                return RedirectToAction(nameof(ChooseVisitType));
            }

            Console.WriteLine("visitData");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(visitData));

            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key}: {Request.Form[key]}");
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
                //return View(visitSchedule); should be like that?
                return View();
            }

            // create schhedule
            _context.VisitSchedule.Add(visitSchedule);
            await _context.SaveChangesAsync();

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(TempData));
            var visit = new Visit
            {
                DoctorId = visitData.DoctorId,
                Doctor = await _context.Doctor.FindAsync(visitData.DoctorId),
                PatientId = visitData.PatientId,
                Patient = await _context.Patient.FindAsync(visitData.PatientId),
                VisitScheduleId = visitSchedule.Id,
                VisitType = TempData["VisitType"]?.ToString(),
                CreatedAt = DateTime.Now,
            };

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(visit));
            Console.WriteLine("Creating visit...");

            var isVisitValid = Validator.TryValidateObject(visit, new ValidationContext(visit), null, true);
            if (isVisitValid)
            {
                Console.WriteLine("Visit is valid.");
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
