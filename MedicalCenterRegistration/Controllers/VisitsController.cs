// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using Humanizer;
using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Helpers;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels.Visits;
using MedicalCenterRegistration.Services;
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
        private readonly VisitsService _visitsService;
        private readonly ILogger<VisitsController> _logger;

        public VisitsController(ApplicationDbContext context, PatientService patientService, VisitsService visitsService, ILogger<VisitsController> logger)
        {
            _context = context;
            _patientService = patientService;
            _visitsService = visitsService;
            _logger = logger;
        }

        // GET: Visits
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> Index()
        {

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var visits = await _context.Visit
                    .Include(v => v.Doctor).ThenInclude(d => d.Image)
                    .Include(v => v.Patient)
                    .Include(v => v.VisitSchedule)
                    .Where(v => v.Patient.UserId == loggedInUserId)
                    .OrderByDescending(v => v.Id) // newest visits first
                    .ToListAsync();

            return View(visits);
        }


        // GET: AllVisits
        [Authorize(Roles = Roles.AdminAndReceptionist)]
        public async Task<IActionResult> AllVisits()
        {
            var applicationDbContext = _context.Visit
                .Include(v => v.Doctor)
                .Include(v => v.Patient)
                .Include(v => v.VisitSchedule)
                .OrderByDescending(v => v.Id); // newest visits first

            return View(await applicationDbContext.ToListAsync());
        }


        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> DoctorVisits()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var visits = await _context.Visit
                .Include(v => v.Doctor)
                .Include(v => v.Patient)
                .Include(v => v.VisitSchedule)
                .Where(v => v.Doctor.UserId == userId)
                .OrderByDescending(v => v.Id) // newest visits first
                .ToListAsync();

            return View(visits);
        }

        // AJAX endpoint for DataTables to fetch visits data
        [HttpPost]
        [Authorize(Roles = Roles.AdminAndReceptionist)]
        public async Task<IActionResult> GetVisits()
        {
            var request = DataTableHelper.GetRequest(Request);
            var query = _context.Visit.AsQueryable();
            var recordsTotal = await query.CountAsync();

            query = query.Include(v => v.Doctor).Include(v => v.Patient).Include(v => v.VisitSchedule);

            //  search by patient's and doctor's info
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where
                    (u => u.Patient.Name.Contains(request.SearchValue) ||
                     u.Patient.LastName.Contains(request.SearchValue) ||
                     u.Doctor.Name.Contains(request.SearchValue) ||
                     u.Doctor.LastName.Contains(request.SearchValue) ||
                     u.Patient.PeselNumber.Contains(request.SearchValue)
                     );
            }


            var recordsFiltered = await query.CountAsync();

            query = query.ApplySorting(request);

            var visits = query
                .Skip(request.Start)
                .Take(request.Length)
            .ToList();

            var vm = new List<AdminReceptionistVisitViewModel>();

            var isReceptionist = User.IsInRole(Roles.Receptionist);

            foreach (var visit in visits)
            {
                var isCancellable = isReceptionist && 
                    visit.Status == Status.Pending &&
                    (visit.VisitSchedule.VisitDate > DateOnly.FromDateTime(DateTime.Now) ||
                     (visit.VisitSchedule.VisitDate == DateOnly.FromDateTime(DateTime.Now) && visit.VisitSchedule.VisitTimeStart > TimeOnly.FromDateTime(DateTime.Now)));

                vm.Add(new AdminReceptionistVisitViewModel
                {
                    Id = visit.Id,
                    Patient = "{0} {1}".FormatWith(visit.Patient.Name, visit.Patient.LastName),
                    Doctor = "{0} {1}".FormatWith(visit.Doctor.Name, visit.Doctor.LastName),
                    Date = visit.VisitSchedule.VisitDate,
                    Time = "{0} - {1}".FormatWith(visit.VisitSchedule.VisitTimeStart.ToString("HH:mm"), visit.VisitSchedule.VisitTimeEnd.ToString("HH:mm")),
                    VisitType = visit.VisitType,
                    VisitStatus = EnumHelper.GetDisplayName(visit.Status),
                    IsCancellable = isCancellable
                });
            }

            var response = DataTableHelper.CreateResponse(
                request,
                recordsTotal,
                recordsFiltered,
                vm
            );

            return Json(response);
        }



        [HttpGet]
        [Authorize(Roles = Roles.ReceptionistAndPatient)]
        public async Task<IActionResult> VisitsLimit()
        {
            return View();
        }


        //GET: ChoosePatient
        [HttpGet]
        [Authorize(Roles = Roles.Receptionist)]
        public async Task<IActionResult> ChoosePatient()
        {
            return View();
        }


        // GET: ChooseSpecializationType
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChooseSpecializationType(int? patientId)
        {

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            bool hasPatientInfo;

            switch (userRole)
            {
                case Roles.Receptionist:
                    hasPatientInfo = await _patientService.HasPatientEntryByIdAsync(patientId);
                    if (!hasPatientInfo)
                    {
                        return RedirectToAction("CreateForUser", "Patients");
                    }
                    var hasPatientReachedVisitsLimit = await _visitsService.HasPatientReachedActiveVisitsLimit(patientId);
                    if (hasPatientReachedVisitsLimit)
                    {
                        return RedirectToAction(nameof(VisitsLimit));
                    }
                    break;
                case Roles.Patient:
                    hasPatientInfo = await _patientService.HasPatientEntryAsync(User);
                    if (!hasPatientInfo)
                    {
                        return RedirectToAction("Create", "Patients", new { returnUrl = Url.Action(nameof(ChooseSpecializationType)) });
                    }
                    var hasUserReachedVisitsLimit = await _visitsService.HasUserReachedActiveVisitsLimit(userId);
                    if (hasUserReachedVisitsLimit)
                    {
                        return RedirectToAction(nameof(VisitsLimit));
                    }
                    break;
                default:
                    return Forbid();
            }


            var specializations = await _context.DoctorSpecialization
                .GroupBy(ds => new { ds.Specialization.Id, ds.Specialization.Name })
                .Select(g => new SpecializationWithDoctorCount
                {
                    SpecializationId = g.Key.Id,
                    SpecializationName = g.Key.Name,
                    DoctorCount = g.Count()
                })
                .ToListAsync();


            var viewModel = new ChooseDoctorSpecializationViewModel
            {
                Specializations = specializations,
                patientId = patientId
            };

            return View(viewModel);
        }

        // POST: Visits
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSpecializationType(string specializationId, string? patientId)
        {
            if (string.IsNullOrEmpty(specializationId))
            {
                ModelState.AddModelError("", "Please select a visit type.");
                return View(nameof(ChooseSpecializationType));
            }

            TempData["SpecializationId"] = specializationId;
            TempData["PatientId"] = patientId;

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
                .Include(v => v.Doctor).ThenInclude(d => d.Image)
                .Include(v => v.Patient)
                .Include(v => v.VisitSchedule)
                .Include(v => v.VisitSummary).ThenInclude(vs => vs.Files)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (visit == null)
            {
                return NotFound();
            }

            // Sprawdzenie, czy zalogowany użytkownik ma dostęp do wizyty
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isVisitDoctor = visit.Doctor.UserId == loggedInUserId;

            if (visit.Patient.UserId != loggedInUserId && !isVisitDoctor)
            {
                return Forbid(); // Brak dostępu
            }


            var doctorSpecializations = await _context.DoctorSpecialization
                .Where(ds => ds.DoctorId == visit.DoctorId)
                .Include(ds => ds.Specialization)
                .ToListAsync();

            string? goBackUrl = null;

            if (User.IsInRole(Roles.Patient))
            {
                goBackUrl = Url.Action(nameof(Index), "Visits");
            }
            else if (User.IsInRole(Roles.Doctor))
            {
                goBackUrl = Url.Action("DoctorVisits", "Visits");
            }
            else
            {
                goBackUrl = Url.Action(nameof(Index), "Visits");
            }

            VisitDetailsViewModel viewModel = new VisitDetailsViewModel
            {
                VisitId = visit.Id,
                Status = visit.Status,
                VisitScheduleDate = visit.VisitSchedule.VisitDate,
                VisitTimeStart = visit.VisitSchedule.VisitTimeStart,
                DoctorFullName = "{0} {1}".FormatWith(visit.Doctor.Name, visit.Doctor.LastName),
                DoctorImage = visit.Doctor.Image,
                DoctorSpecializations = doctorSpecializations.Select(ds => ds.Specialization.Name).ToList(),
                PatientFullName = "{0} {1}".FormatWith(visit.Patient.Name, visit.Patient.LastName),
                VisitDate = visit.VisitSchedule.VisitDate,
                FormattedVisitTime = "{0} - {1}".FormatWith(visit.VisitSchedule.VisitTimeStart.ToString("HH:mm"), visit.VisitSchedule.VisitTimeEnd.ToString("HH:mm")),
                Description = visit.VisitSummary?.Description ?? "",
                Files = visit.VisitSummary?.Files ?? new List<UserFile>(),
                CreatedAt = visit.CreatedAt,
                UpdatedAt = visit.UpdatedAt,
                GoBackUrl = goBackUrl,
            };

            return View(viewModel);
        }

        // GET: Visits/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {

            var specializationId = TempData.Peek("SpecializationId");
            var patientId = TempData.Peek("PatientId")?.ToString();


            if (specializationId == null)
            {
                return RedirectToAction(nameof(ChooseSpecializationType));
            }

            Patient patient = null;
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            switch (userRole)
            {
                case Roles.Receptionist:
                    if (patientId == null)
                    {
                        return BadRequest("Patient ID is required.");
                    }

                    patient = await _patientService.GetPatientByIdAsync(int.Parse(patientId));
                    if (patient == null)
                    {
                        ModelState.AddModelError("", "Patient not found.");
                        return RedirectToAction("Create", "Patients");
                    }
                    break;
                case Roles.Patient:
                    patient = await _patientService.GetPatientByUserIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (patient == null)
                    {
                        ModelState.AddModelError("", "Patient not found.");
                        return RedirectToAction("Create", "Patients");
                    }
                    break;
                default:
                    return Forbid();
            }



            var hasReachedVisitsLimit = await _visitsService.HasPatientReachedActiveVisitsLimit(patient?.Id);
            if (hasReachedVisitsLimit)
            {
                return RedirectToAction(nameof(VisitsLimit));
            }


            var doctorsForSpecialization = await _context.DoctorSpecialization
                .Where(ds => ds.SpecializationId == int.Parse(specializationId.ToString()))
                .Include(ds => ds.Doctor).ThenInclude(d => d.Image)
                .Select(ds => ds.Doctor)
                .ToListAsync();

            var doctorIds = doctorsForSpecialization
                .Select(d => d.Id)
                .ToList();

            var doctorSchedules = await _context.Visit
                .Where(v => doctorIds.Contains(v.DoctorId))
                .Select(v => new
                {
                    v.DoctorId,
                    Schedule = new
                    {
                        v.VisitSchedule.Id,
                        v.VisitSchedule.VisitDate,
                        v.VisitSchedule.VisitTimeStart,
                        v.VisitSchedule.VisitTimeEnd
                    }
                })
                .ToListAsync();

            var doctorScheduledVisits = doctorSchedules
                .GroupBy(v => v.DoctorId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(v => new VisitSchedule
                    {
                        Id = v.Schedule.Id,
                        VisitDate = v.Schedule.VisitDate,
                        VisitTimeStart = v.Schedule.VisitTimeStart,
                        VisitTimeEnd = v.Schedule.VisitTimeEnd
                    }).ToList()
                );

            var viewModel = new CreateVisitCreationViewModel
            {
                Doctors = doctorsForSpecialization,
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

            var hasReachedVisitsLimit = await _visitsService.HasPatientReachedActiveVisitsLimit(visitData.PatientId);
            if (hasReachedVisitsLimit)
            {
                return RedirectToAction(nameof(VisitsLimit));
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

            var patient = await _patientService.GetPatientByIdAsync(visitData.PatientId);

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
                VisitType = "Wizyta kontrolna", /* TODO decide if we need this field at all */
                Status = Enums.Status.Pending,
                CreatedAt = DateTime.Now,
            };


            var isVisitValid = Validator.TryValidateObject(visit, new ValidationContext(visit), null, true);
            if (!isVisitValid)
            {
                ModelState.AddModelError("", "Invalid visit data.");
                return View();
            }

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;



            _context.Visit.Add(visit);
            await _context.SaveChangesAsync();


            if (userRole == Roles.Receptionist)
            {
                return RedirectToAction(nameof(AllVisits));
            }


            return RedirectToAction(nameof(Index));
        }

        // GET: Visits/Edit/5
        [Authorize(Roles = "Receptionist, Admin")]
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

        // GET: Visits/Cancel/5
        [Authorize(Roles = Roles.ReceptionistAndPatient)]
        public async Task<IActionResult> Cancel(int? id)
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

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isReceptionist = User.IsInRole(Roles.Receptionist);
            var isPatientOwner = visit.Patient.UserId == loggedInUserId;

            if (!isReceptionist && !isPatientOwner)
            {
                return Forbid();
            }

            var visitDateTime = visit.VisitSchedule.VisitDate.ToDateTime(visit.VisitSchedule.VisitTimeStart);
            var isInFuture = visitDateTime > DateTime.Now;

            if (visit.Status != Status.Pending || !isInFuture)
            {
                TempData["Error"] = "Tylko zaplanowane wizyty, które jeszcze się nie rozpoczęły, mogą być anulowane.";
                return RedirectToAction(nameof(Index));
            }

            return View(visit);
        }

        // POST: Visits/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.ReceptionistAndPatient)]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var visit = await _context.Visit
                .Include(v => v.Patient)
                .Include(v => v.VisitSchedule)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null)
            {
                return NotFound();
            }

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isReceptionist = User.IsInRole(Roles.Receptionist);
            var isPatientOwner = visit.Patient.UserId == loggedInUserId;

            if (!isReceptionist && !isPatientOwner)
            {
                return Forbid();
            }

            var visitDateTime = visit.VisitSchedule.VisitDate.ToDateTime(visit.VisitSchedule.VisitTimeStart);
            var isInFuture = visitDateTime > DateTime.Now;

            if (visit.Status != Status.Pending || !isInFuture)
            {
                TempData["Error"] = "Tylko zaplanowane wizyty, które jeszcze się nie rozpoczęły, mogą być anulowane.";
                if (isReceptionist)
                {
                    return RedirectToAction(nameof(AllVisits));
                }
                return RedirectToAction(nameof(Index));
            }

            visit.Status = Status.Cancelled;
            visit.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wizyta została pomyślnie anulowana.";

            if (isReceptionist)
            {
                return RedirectToAction(nameof(AllVisits));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
