// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Helpers;
using MedicalCenterRegistration.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    [Authorize(Roles = Roles.AdminAndReceptionist)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserManagementController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /UserManagement/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains("Admin"))
            {
                ViewData["Roles"] = new List<string> { "Receptionist", "Patient", "Admin" };
            }
            else if (roles.Contains("Receptionist"))
            {
                ViewData["Roles"] = new List<string> { "Patient" };
            }
            else
            {
                return Forbid(); // inne role nie mają dostępu
            }

            return View();
        }

        // POST: /UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Ustal, jakie role może tworzyć bieżący użytkownik
            List<string> allowedRoles;
            if (roles.Contains("Admin"))
                allowedRoles = new List<string> { "Receptionist", "Patient", "Admin" }; //do tworzenia lekarzy jest osobna logika
            else if (roles.Contains("Receptionist"))
                allowedRoles = new List<string> { "Patient" };
            else
                return Forbid();

            ViewData["Roles"] = allowedRoles;

            if (!ModelState.IsValid)
                return View(model);

            if (!allowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError("", "Nie masz uprawnień do tworzenia użytkowników z tą rolą.");
                return View(model);
            }

            // Tworzenie użytkownika
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Przypisanie roli
            await _userManager.AddToRoleAsync(user, model.Role);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: /UserManagement/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // AJAX endpoint for DataTables to fetch user data
        [HttpPost]
        [Authorize(Roles = Roles.AdminAndReceptionist)]
        public async Task<IActionResult> GetUsers()
        {
            var request = DataTableHelper.GetRequest(Request);

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var isReceptionist = currentUserRoles.Contains(Roles.Receptionist);

            var query = _userManager.Users.AsQueryable();

            // recepcjonista widzi tylko pacjentów
            if (isReceptionist)
            {
                var patientRoleId = await _context.Roles.Where(r => r.Name == Roles.Patient).Select(r => r.Id).FirstOrDefaultAsync();

                if (patientRoleId != null)
                {
                    query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == patientRoleId));
                }
            }

            var recordsTotal = await query.CountAsync();

            //  search by email
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(u => u.Email.Contains(request.SearchValue));
            }

            var recordsFiltered = await query.CountAsync();

            query = query.ApplySorting(request);

            var users = query
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            var vm = new List<UserListItemViewModel>();


            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var userRole = userRoles.FirstOrDefault() ?? "Brak roli";

                vm.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = userRole,
                    CreatedAt = DateTime.MinValue
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
    }

}
