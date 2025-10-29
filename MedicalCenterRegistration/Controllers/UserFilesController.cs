// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
using Microsoft.AspNetCore.Mvc;

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

            if (!System.IO.File.Exists(fullFilePath))
                return NotFound();

            var contentType = "application/octet-stream";
            return PhysicalFile(fullFilePath, contentType, fileName);
        }

        private bool UserFileExists(int id)
        {
            return _context.UserFile.Any(e => e.Id == id);
        }
    }
}
