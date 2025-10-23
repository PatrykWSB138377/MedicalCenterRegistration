// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    [Route("Visits/{visitId}/VisitsSummary")]
    public class VisitSummariesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitSummariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VisitSummaries
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.VisitSummary.Include(v => v.Visit);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: VisitSummaries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitSummary = await _context.VisitSummary
                .Include(v => v.Visit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visitSummary == null)
            {
                return NotFound();
            }

            return View(visitSummary);
        }

        // GET: VisitSummaries/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? visitId)
        {
            if (visitId == null)
            {
                return NotFound();
            }

            var visit = await _context.Visit
                            .Where(v => v.Id == visitId && v.Status == Status.Pending) // if the visit is not pending it will not allow to create summary
                            .FirstOrDefaultAsync();

            var existingSummary = await _context.VisitSummary
               .FirstOrDefaultAsync(vs => vs.VisitId == visitId);

            if (visit == null || existingSummary != null)
            {
                return NotFound();
            }

            ViewData["VisitId"] = visitId;
            return View();
        }

        // POST: VisitSummaries/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? visitId, [Bind("Description,Files")] VisitSummary visitSummary)
        {
            var visit = await _context.Visit
                              .Where(v => v.Id == visitId && v.Status == Status.Pending) // if the visit is not pending it will not allow to create summary
                              .FirstOrDefaultAsync();

            var existingSummary = await _context.VisitSummary
                .FirstOrDefaultAsync(vs => vs.VisitId == visitId);

            if (visit == null || existingSummary != null)
            {
                return BadRequest("Nie znaleziono wizyty.");
            }



            visit.Status = Status.Finished;
            visit.VisitSummary = visitSummary;
            visitSummary.VisitId = visit.Id;
            visitSummary.Visit = visit;

            if (ModelState.IsValid)
            {
                _context.Add(visitSummary);
                _context.Update(visit);
                await _context.SaveChangesAsync();
                return RedirectToAction("DoctorVisits", "Visits", null);
            }
            ViewData["VisitId"] = visitId;
            return View(visitSummary);
        }

        // GET: VisitSummaries/Edit/5
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(int? visitId)
        {
            Console.WriteLine("ENTERED *****************************************");
            if (visitId == null)
            {
                return NotFound();
            }


            var visitSummary = await _context.VisitSummary.FindAsync(visitId);
            if (visitSummary == null)
            {
                return NotFound();
            }
            ViewData["VisitId"] = new SelectList(_context.Visit, "Id", "Id", visitSummary.VisitId);
            return View(visitSummary);
        }

        // POST: VisitSummaries/Edit/5
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VisitId,Description,CreatedAt,UpdatedAt")] VisitSummary visitSummary)
        {
            if (id != visitSummary.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(visitSummary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitSummaryExists(visitSummary.Id))
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
            ViewData["VisitId"] = new SelectList(_context.Visit, "Id", "Id", visitSummary.VisitId);
            return View(visitSummary);
        }

        // GET: VisitSummaries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var visitSummary = await _context.VisitSummary
                .Include(v => v.Visit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (visitSummary == null)
            {
                return NotFound();
            }

            return View(visitSummary);
        }

        // POST: VisitSummaries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visitSummary = await _context.VisitSummary.FindAsync(id);
            if (visitSummary != null)
            {
                _context.VisitSummary.Remove(visitSummary);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VisitSummaryExists(int id)
        {
            return _context.VisitSummary.Any(e => e.Id == id);
        }
    }
}
