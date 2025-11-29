// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Consts;
using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterRegistration.Services;

public class VisitsService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public VisitsService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> HasUserReachedActiveVisitsLimit(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        return await _context.Visit
            .Where(v => v.Patient.UserId == userId && v.Status == Status.Pending)
            .CountAsync() >= VisitsConsts.PATIENT_MAX_ACTIVE_VISITS;
    }

    public async Task<bool> HasPatientReachedActiveVisitsLimit(int? patientId)
    {
        if (patientId == null)
            return false;

        return await _context.Visit
            .Where(v => v.PatientId == patientId && v.Status == Status.Pending)
            .CountAsync() >= VisitsConsts.PATIENT_MAX_ACTIVE_VISITS;
    }

}
