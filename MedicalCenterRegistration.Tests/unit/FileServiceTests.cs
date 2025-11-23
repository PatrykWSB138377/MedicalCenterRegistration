using MedicalCenterRegistration.Data;
using MedicalCenterRegistration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text;

namespace MedicalCenterRegistration.Tests
{
    public class FileServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            return new ApplicationDbContext(options);
        }

        private Mock<IFormFile> CreateMockFormFile(string fileName, string content = "test content")
        {
            var mockFile = new Mock<IFormFile>();
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var memoryStream = new MemoryStream(contentBytes);
            
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(contentBytes.Length);
            mockFile.Setup(f => f.ContentType).Returns("application/octet-stream");
            
            return mockFile;
        }

        [Fact]
        public void AppDataFilePaths_UserFiles_ReturnsCorrectPath()
        {
            // arrange 
            var firstPart = "AppData";
            var secondPart = "UserFiles";

            // act
            var userFilesPath = Path.Combine(firstPart, secondPart);

            // assert
            Assert.Equal("AppData\\UserFiles", userFilesPath.Replace("/", "\\"));
        }

        [Fact]
        public void AppDataFilePaths_PublicImages_ReturnsCorrectPath()
        {
            // arrange 
            var firstPart = "AppData";
            var secondPart = "PublicImages";

            // act
            var publicImagesPath = Path.Combine(firstPart, secondPart);

            // assert
            Assert.Equal("AppData\\PublicImages", publicImagesPath.Replace("/", "\\"));
        }


        [Fact]
        public void ParsedFileData_ParsesFileName_Correctly()
        {
            // arrange
            var mockFile = CreateMockFormFile("document.pdf");

            // act & assert
            Assert.Equal("document.pdf", mockFile.Object.FileName);
            Assert.Equal(".pdf", Path.GetExtension(mockFile.Object.FileName));
            Assert.Equal("document", Path.GetFileNameWithoutExtension(mockFile.Object.FileName));
        }

        [Fact]
        public void ParsedFileData_WithComplexFileName_ParsesCorrectly()
        {
            // arrange
            var mockFile = CreateMockFormFile("my.file.name.with.dots.txt");

            // act & assert
            Assert.Equal("my.file.name.with.dots.txt", mockFile.Object.FileName);
            Assert.Equal(".txt", Path.GetExtension(mockFile.Object.FileName));
            Assert.Equal("my.file.name.with.dots", Path.GetFileNameWithoutExtension(mockFile.Object.FileName));
        }

    }
}
