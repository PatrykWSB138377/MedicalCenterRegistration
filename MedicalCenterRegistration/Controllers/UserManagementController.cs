using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using MedicalCenterRegistration.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalCenterRegistration.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
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
            // Admin widzi wszystkich
            // Recepcjonista tylko pacjentów
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            var users = _userManager.Users.ToList();
            var model = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var mainRole = userRoles.FirstOrDefault() ?? "Brak roli";

                if (roles.Contains("Receptionist") && mainRole != "Patient")
                    continue; // recepcjonista nie widzi innych użytkowników

                

                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = mainRole,
                    CreatedAt =  DateTime.MinValue
                });
            }

            return View(model);
        }
    }
}
