// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using MedicalCenterRegistration.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Controllers
{
    public class UserFilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserFilesController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult DownloadFile(string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(filePath))
                return NotFound();

            var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);


            var fileInDb = _context.UserFile.Include(uf => uf.Owners).Where(UserFile => UserFile.FilePath == filePath).FirstOrDefault();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!System.IO.File.Exists(fullFilePath) || fileInDb == null)
                return NotFound();


            var ownerIds = fileInDb.Owners.Select(owner => owner.UserId).ToList();


            if (!ownerIds.Contains(userId))
            {
                return Unauthorized();
            }

            var contentType = "application/octet-stream";
            return PhysicalFile(fullFilePath, contentType, fileName);
        }

        private bool UserFileExists(int id)
        {
            return _context.UserFile.Any(e => e.Id == id);
        }
    }
}
