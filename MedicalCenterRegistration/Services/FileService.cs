// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using MedicalCenterRegistration.Data;
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


        private static UserFile FormFileToUserFile(IFormFile formFile)
        {
            var parsedData = new ParsedFileData(formFile);

            return new UserFile
            {
                FileName = parsedData.fileNameWithExt,
                FilePath = Path.Combine(AppDataFilePaths.UserFiles, parsedData.parsedFileName),
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

        public async static Task<List<UserFile>> UploadUserFiles(List<IFormFile> formFiles, List<string> ownerIds, ApplicationDbContext context)
        {
            List<UserFile> userFiles = new List<UserFile>();

            if (formFiles == null || formFiles.Count == 0)
            {
                return userFiles;
            }

            await UploadFiles(formFiles, (formFile) =>
            {
                UserFile userFile = FormFileToUserFile(formFile);
                context.UserFile.Add(userFile);

                context.SaveChanges();

                foreach (var ownerId in ownerIds)
                {
                    var owner = new UserFileOwner
                    {
                        FileId = userFile.Id,
                        UserId = ownerId
                    };
                    context.Set<UserFileOwner>().Add(owner);
                }

                userFiles.Add(userFile);
                return userFile.FilePath;
            });

            await context.SaveChangesAsync();

            return userFiles;
        }


        public async static Task<List<PublicImage>> UploadPublicImages(List<IFormFile> formFiles)
        {
            List<PublicImage> publicImages = new List<PublicImage>();

            if (formFiles == null || formFiles.Count == 0)
            {
                return publicImages;
            }

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

        private async static Task DeleteFileAsync(string filePath)
        {
            var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

            if (System.IO.File.Exists(fullFilePath))
            {
                System.IO.File.Delete(fullFilePath);
            }
        }

        public async static Task DeleteUserFilesAsync(List<UserFile> userFiles, ApplicationDbContext context)
        {
            foreach (var userFile in userFiles)
            {
                await DeleteFileAsync(userFile.FilePath);
            }

            context.UserFileOwner.RemoveRange(context.UserFileOwner.Where(owner => userFiles.Select(uf => uf.Id).Contains(owner.FileId)));
            context.UserFile.RemoveRange(userFiles);
        }
    }

}
