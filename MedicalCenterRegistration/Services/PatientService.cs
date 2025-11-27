// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class PatientService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PatientService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> HasPatientEntryAsync(ClaimsPrincipal user)
    {
        if (user == null || !user.Identity.IsAuthenticated)
            return false;

        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrEmpty(userId))
            return false;

        return await _context.Patient.AnyAsync(p => p.UserId == userId);
    }

    public async Task<bool> HasPatientEntryByIdAsync(int? patientId)
    {
        if (patientId == null)
            return false;

        return await _context.Patient.AnyAsync(p => p.Id == patientId);
    }

    public async Task<Patient?> GetPatientByIdAsync(int? patientId) => await _context.Patient.FirstOrDefaultAsync(p => p.Id == patientId);

    public async Task<Patient?> GetPatientByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return null;

        return await _context.Patient.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
