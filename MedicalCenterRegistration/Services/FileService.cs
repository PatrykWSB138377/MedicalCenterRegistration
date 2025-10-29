// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Services
{
    static class FileService
    {
        public static string EncodeFileName(string originalFileName)
        {
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            var shortId = Convert.ToBase64String(bytes)
                                 .Replace("/", "_")
                                 .Replace("+", "-")
                                 .Substring(0, 22);

            return shortId;
        }


        public static UserFile FormFileToUserFile(IFormFile formFile, string userId)
        {
            var fileNameWithExt = formFile.FileName;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameWithExt);
            var extension = Path.GetExtension(fileNameWithExt);
            var encodedFileName = FileService.EncodeFileName(fileNameWithoutExt);
            var parsedFileName = $"{encodedFileName}{extension}";

            return new UserFile
            {
                FileName = fileNameWithExt,
                FilePath = Path.Combine("AppData", "UserFiles", parsedFileName),
                OwnerUserId = userId,
            };
        }

    }
}
