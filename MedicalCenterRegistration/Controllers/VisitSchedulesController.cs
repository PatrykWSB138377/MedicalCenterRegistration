// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class VisitSchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VisitSchedules
        public async Task<IActionResult> Index()
        {
            return View(await _context.VisitSchedule.ToListAsync());
        }

        // GET: VisitSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitSchedule = await _context.VisitSchedule
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visitSchedule == null)
            {
                return NotFound();
            }

            return View(visitSchedule);
        }

        // GET: VisitSchedules/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VisitSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VisitDate,VisitTimeStart,VisitTimeEnd")] VisitSchedule visitSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(visitSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(visitSchedule);
        }

        // GET: VisitSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitSchedule = await _context.VisitSchedule.FindAsync(id);
            if (visitSchedule == null)
            {
                return NotFound();
            }
            return View(visitSchedule);
        }

        // POST: VisitSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VisitDate,VisitTimeStart,VisitTimeEnd")] VisitSchedule visitSchedule)
        {
            if (id != visitSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(visitSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitScheduleExists(visitSchedule.Id))
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
            return View(visitSchedule);
        }

        // GET: VisitSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitSchedule = await _context.VisitSchedule
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visitSchedule == null)
            {
                return NotFound();
            }

            return View(visitSchedule);
        }

        // POST: VisitSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visitSchedule = await _context.VisitSchedule.FindAsync(id);
            if (visitSchedule != null)
            {
                _context.VisitSchedule.Remove(visitSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VisitScheduleExists(int id)
        {
            return _context.VisitSchedule.Any(e => e.Id == id);
        }
    }
}
