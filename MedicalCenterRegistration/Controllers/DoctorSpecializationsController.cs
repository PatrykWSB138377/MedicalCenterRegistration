// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class DoctorSpecializationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorSpecializationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DoctorSpecializations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DoctorSpecialization.Include(d => d.Doctor).Include(d => d.Specialization);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DoctorSpecializations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctorSpecialization = await _context.DoctorSpecialization
                .Include(d => d.Doctor)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctorSpecialization == null)
            {
                return NotFound();
            }

            return View(doctorSpecialization);
        }

        // GET: DoctorSpecializations/Create
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName");
            ViewData["SpecializationId"] = new SelectList(_context.Specialization, "Id", "Id");
            return View();
        }

        // POST: DoctorSpecializations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DoctorId,SpecializationId")] DoctorSpecialization doctorSpecialization)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctorSpecialization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName", doctorSpecialization.DoctorId);
            ViewData["SpecializationId"] = new SelectList(_context.Specialization, "Id", "Id", doctorSpecialization.SpecializationId);
            return View(doctorSpecialization);
        }

        // GET: DoctorSpecializations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctorSpecialization = await _context.DoctorSpecialization.FindAsync(id);
            if (doctorSpecialization == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName", doctorSpecialization.DoctorId);
            ViewData["SpecializationId"] = new SelectList(_context.Specialization, "Id", "Id", doctorSpecialization.SpecializationId);
            return View(doctorSpecialization);
        }

        // POST: DoctorSpecializations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DoctorId,SpecializationId")] DoctorSpecialization doctorSpecialization)
        {
            if (id != doctorSpecialization.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctorSpecialization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorSpecializationExists(doctorSpecialization.Id))
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
            ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName", doctorSpecialization.DoctorId);
            ViewData["SpecializationId"] = new SelectList(_context.Specialization, "Id", "Id", doctorSpecialization.SpecializationId);
            return View(doctorSpecialization);
        }

        // GET: DoctorSpecializations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctorSpecialization = await _context.DoctorSpecialization
                .Include(d => d.Doctor)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctorSpecialization == null)
            {
                return NotFound();
            }

            return View(doctorSpecialization);
        }

        // POST: DoctorSpecializations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctorSpecialization = await _context.DoctorSpecialization.FindAsync(id);
            if (doctorSpecialization != null)
            {
                _context.DoctorSpecialization.Remove(doctorSpecialization);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorSpecializationExists(int id)
        {
            return _context.DoctorSpecialization.Any(e => e.Id == id);
        }
    }
}
