// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels.VisitSummaries;
using MedicalCenterRegistration.Services;
using Microsoft.AspNetCore.Mvc;
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
            // TODO: you should only be able to create a summary for visits that have at least happened (date in the past or now)
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

            return View();
        }

        // POST: VisitSummaries/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? visitId, [Bind("Description,UploadedFiles")] CreateVisitSummaryViewModel visitSummary)
        {
            // TODO: you should only be able to create a summary for visits that have at least happened (date in the past or now)
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploadedFiles = await FileService.UploadUserFiles(visitSummary.UploadedFiles, userId);

            visit.VisitSummary = new VisitSummary
            {
                Description = visitSummary.Description,
                Files = uploadedFiles,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Visit = visit,
                VisitId = visit.Id
            };

            if (ModelState.IsValid)
            {
                _context.Update(visit);
                await _context.SaveChangesAsync();
                return RedirectToAction("DoctorVisits", "Visits", null);
            }

            return View(visitSummary);
        }

        // GET: VisitSummaries/Edit/5
        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(int? visitId)
        {
            if (visitId == null)
            {
                return NotFound();
            }


            var visitSummary = await _context.VisitSummary.FirstOrDefaultAsync(vs => vs.VisitId == visitId);
            if (visitSummary == null)
            {
                return NotFound();
            }

            var viewModel = new CreateVisitSummaryViewModel
            {
                Id = visitSummary.Id,
                VisitId = visitSummary.VisitId,
                Description = visitSummary.Description,
            };

            return View(viewModel);
        }

        // POST: VisitSummaries/Edit/5
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int visitId, [Bind("Id,VisitId,Description,UploadedFiles")] CreateVisitSummaryViewModel visitSummary)
        {
            Console.WriteLine("********* EDIT **********");
            Console.WriteLine("********* EDIT **********");
            Console.WriteLine("********* EDIT **********");
            Console.WriteLine("********* EDIT **********");

            var visit = await _context.Visit
                              .Include(v => v.VisitSummary)
                              .Where(v => v.Id == visitId && v.Status == Status.Finished)
                              .FirstOrDefaultAsync();

            if (visit?.VisitSummary?.Id != visitSummary.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    visit.VisitSummary.Description = visitSummary.Description;
                    visit.VisitSummary.UpdatedAt = DateTime.UtcNow;

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var uploadedFiles = await FileService.UploadUserFiles(visitSummary.UploadedFiles, userId);

                    visit.VisitSummary.Files.AddRange(uploadedFiles);


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
                return RedirectToAction("DoctorVisits", "Visits");
            }

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
