// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Services
{
    static class FileService
    {
        public static string EncodeFileName(string originalFileName)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(originalFileName));
        }


    }
}
