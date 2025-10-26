// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    [Authorize(Roles = "Admin")]
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
            var model = new DoctorSpecializationCreateViewModel
            {
                Doctor = _context.Doctor
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.Name} {d.LastName}"
                    }).ToList(),

                Specialization = _context.Specialization
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name
                    }).ToList()
            };

            return View(model);
        }


        // POST: DoctorSpecializations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorSpecializationCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Sprawdzenie, czy taka para (DoctorId, SpecializationId) już istnieje
                bool alreadyExists = await _context.DoctorSpecialization
                    .AnyAsync(ds => ds.DoctorId == model.DoctorId && ds.SpecializationId == model.SpecializationId);

                if (alreadyExists)
                {
                    ModelState.AddModelError("", "Ten lekarz już ma przypisaną tę specjalizację.");
                }
                else
                {
                    var entity = new DoctorSpecialization
                    {
                        DoctorId = model.DoctorId,
                        SpecializationId = model.SpecializationId
                    };

                    _context.Add(entity);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            //  Ponowne wypełnienie list po nieudanym zapisie
            model.Doctor = await _context.Doctor
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.Name} {d.LastName}"
                }).ToListAsync();

            model.Specialization = await _context.Specialization
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();

            return View(model);
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
                // Sprawdź, czy istnieje inny wpis z tą samą kombinacją (DoctorId, SpecializationId)
                bool duplicateExists = await _context.DoctorSpecialization
                    .AnyAsync(ds => ds.DoctorId == doctorSpecialization.DoctorId
                                 && ds.SpecializationId == doctorSpecialization.SpecializationId
                                 && ds.Id != doctorSpecialization.Id);

                if (duplicateExists)
                {
                    ModelState.AddModelError("", "Ten lekarz już ma przypisaną tę specjalizację.");
                }
                else
                {
                    try
                    {
                        _context.Update(doctorSpecialization);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
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
                }
            }

            // Ponowne wypełnienie list (żeby formularz się poprawnie załadował)
            ViewData["DoctorId"] = new SelectList(_context.Doctor, "Id", "LastName", doctorSpecialization.DoctorId);
            ViewData["SpecializationId"] = new SelectList(_context.Specialization, "Id", "Name", doctorSpecialization.SpecializationId);

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
