// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Models;

namespace MedicalCenterRegistration.Services
{


    static class FileService
    {

        static class AppDataFilePaths
        {
            public static readonly string UserFiles = Path.Combine("AppData", "UserFiles");
            public static readonly string PublicImages = Path.Combine("AppData", "PublicImages");
        }


        class ParsedFileData
        {
            public string fileNameWithExt { get; set; }
            public string fileNameWithoutExt { get; set; }
            public string extension { get; set; }
            public string encodedFileName { get; set; }
            public string parsedFileName { get; set; }

            public ParsedFileData(IFormFile file)
            {
                fileNameWithExt = file.FileName;
                fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameWithExt);
                extension = Path.GetExtension(fileNameWithExt);
                encodedFileName = EncodeFileName(fileNameWithoutExt);
                parsedFileName = $"{encodedFileName}{extension}";
            }
        }

        private static string EncodeFileName(string originalFileName)
        {
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            var shortId = Convert.ToBase64String(bytes)
                                 .Replace("/", "_")
                                 .Replace("+", "-")
                                 .Substring(0, 22);

            return shortId;
        }


        private static UserFile FormFileToUserFile(IFormFile formFile, string userId)
        {
            var parsedData = new ParsedFileData(formFile);

            return new UserFile
            {
                FileName = parsedData.fileNameWithExt,
                FilePath = Path.Combine(AppDataFilePaths.UserFiles, parsedData.parsedFileName),
                OwnerUserId = userId,
            };
        }

        private async static Task UploadFiles(List<IFormFile> files, Func<IFormFile, string> pathGetter)
        {
            foreach (var file in files)
            {
                var uploadPath = pathGetter(file);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), uploadPath);
                var directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
        }

        public async static Task<List<UserFile>> UploadUserFiles(List<IFormFile> formFiles, string userId)
        {
            List<UserFile> userFiles = new List<UserFile>();

            await UploadFiles(formFiles, (formFile) =>
            {
                UserFile userFile = FormFileToUserFile(formFile, userId);
                userFiles.Add(userFile);
                return userFile.FilePath;
            });

            return userFiles;
        }


        public async static Task<List<PublicImage>> UploadPublicImages(List<IFormFile> formFiles)
        {
            List<PublicImage> publicImages = new List<PublicImage>();

            await UploadFiles(formFiles, (formFile) =>
            {
                var preparedFile = new ParsedFileData(formFile);
                publicImages.Add(new PublicImage
                {
                    FileName = preparedFile.parsedFileName,
                    ContentType = formFile.ContentType
                });

                return Path.Combine(AppDataFilePaths.PublicImages, preparedFile.parsedFileName);
            });

            return publicImages;
        }

    }
}
