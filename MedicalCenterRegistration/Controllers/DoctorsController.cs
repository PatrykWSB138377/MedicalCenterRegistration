// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Doctor.Include(d => d.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Doctors/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctor
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
            return View();
        }

        // POST: Doctors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Email,Password,Name,LastName,Description,DateOfBirth,Sex,ImageFile")] CreateDoctorPayloadViewModel doctorData)
        {
            // 🔹 Sprawdź, czy nie przesłano pliku
            if (doctorData.ImageFile == null)
            {
                ModelState.AddModelError(nameof(doctorData.ImageFile), "Zdjęcie jest wymagane.");
            }
            if (!ModelState.IsValid)
            {
                ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
                return View(doctorData);
            }

            if (doctorData.ImageFile != null)
            {
                var fileName = Path.GetFileName(doctorData.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doctorData.ImageFile.CopyToAsync(stream);
                }
            }

            // create Image
            var image = new Image
            {
                ContentType = doctorData.ImageFile.ContentType,
                Base64Data = Convert.ToBase64String(System.IO.File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", doctorData.ImageFile.FileName))),
            };

            _context.Image.Add(image);

            var user = new IdentityUser
            {
                UserName = doctorData.Email,
                Email = doctorData.Email,
                EmailConfirmed = true
            };


            var createUserResult = await _userManager.CreateAsync(user, doctorData.Password);

            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    Console.WriteLine($"Identity error: {error.Description}");
                }

                Console.WriteLine("NOT SUCCEEDED");

                ViewData["SexOptions"] = EnumHelper.GetSelectList<Sex>();
                return View(doctorData);
            }


            await _userManager.AddToRoleAsync(user, "Doctor");

            var doctor = new Doctor
            {
                Name = doctorData.Name,
                LastName = doctorData.LastName,
                DateOfBirth = doctorData.DateOfBirth,
                Description = doctorData.Description,
                Sex = doctorData.Sex,
                CreatedAt = DateTime.Now,
                UserId = user.Id,
                ImageId = image.Id,
                Image = image,
            };

            Console.WriteLine("CREATING DOCTOR");

            try
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                Console.WriteLine("DOCTOR CREATED");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, "Failed to create doctor.");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctor.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", doctor.UserId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,UserId,CreatedAt")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", doctor.UserId);
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctor
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctor.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctor.Remove(doctor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctor.Any(e => e.Id == id);
        }
    }
}
