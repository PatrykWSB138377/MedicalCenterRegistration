using MedicalCenterRegistration.Helpers;
using MedicalCenterRegistration.Models.DataTables;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace MedicalCenterRegistration.Tests
{
    public class DataTableHelperTests
    {
        [Fact]
        public void GetRequest_ParsesHttpRequest_ReturnsDataTableRequest()
        {
            // arrange
            var mockHttpRequest = new Mock<HttpRequest>();
            
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", "1" },
                { "start", "0" },
                { "length", "10" },
                { "search[value]", "test" },
                { "order[0][column]", "1" },
                { "order[0][dir]", "asc" },
                { "columns[0][data]", "Id" },
                { "columns[0][name]", "Id" },
                { "columns[1][data]", "Name" },
                { "columns[1][name]", "Name" }
            });

            mockHttpRequest.Setup(r => r.Form).Returns(formCollection);

            // act
            var result = DataTableHelper.GetRequest(mockHttpRequest.Object);

            // assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Draw);
            Assert.Equal(0, result.Start);
            Assert.Equal(10, result.Length);
            Assert.Equal("test", result.SearchValue);
            Assert.Equal("Name", result.SortColumn);
            Assert.Equal("asc", result.SortDirection);
        }

        [Fact]
        public void CreateResponse_WithValidData_ReturnsDataTableResponse()
        {
            // arrange
            var request = new DataTableRequest
            {
                Draw = 1,
                Start = 0,
                Length = 10
            };
            int totalRecords = 100;
            int filteredRecords = 50;
            var data = new List<string> { "Item1", "Item2", "Item3" };

            // act
            var result = DataTableHelper.CreateResponse(request, totalRecords, filteredRecords, data);

            // assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Draw);
            Assert.Equal(100, result.RecordsTotal);
            Assert.Equal(50, result.RecordsFiltered);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal("Item1", result.Data.First());
        }

        [Fact]
        public void ApplySorting_WithSortColumn_AppliesSorting()
        {
            // arrange
            var testData = new List<TestEntity>
            {
                new TestEntity { Id = 3, Name = "Charlie" },
                new TestEntity { Id = 1, Name = "Alice" },
                new TestEntity { Id = 2, Name = "Bob" }
            }.AsQueryable();

            var request = new DataTableRequest
            {
                SortColumn = "Name",
                SortDirection = "asc"
            };

            // act
            var result = testData.ApplySorting(request).ToList();

            // assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal("Bob", result[1].Name);
            Assert.Equal("Charlie", result[2].Name);
        }

        [Fact]
        public void ApplySorting_WithDescendingOrder_AppliesSortingDescending()
        {
            // arrange
            var testData = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "Alice" },
                new TestEntity { Id = 2, Name = "Bob" },
                new TestEntity { Id = 3, Name = "Charlie" }
            }.AsQueryable();

            var request = new DataTableRequest
            {
                SortColumn = "Id",
                SortDirection = "desc"
            };

            // act
            var result = testData.ApplySorting(request).ToList();

            // assert
            Assert.Equal(3, result.Count);
            Assert.Equal(3, result[0].Id);
            Assert.Equal(2, result[1].Id);
            Assert.Equal(1, result[2].Id);
        }

        [Fact]
        public void ApplySorting_WithNullSortColumn_ReturnsOriginalOrder()
        {
            // arrange
            var testData = new List<TestEntity>
            {
                new TestEntity { Id = 3, Name = "Charlie" },
                new TestEntity { Id = 1, Name = "Alice" },
                new TestEntity { Id = 2, Name = "Bob" }
            }.AsQueryable();

            var request = new DataTableRequest
            {
                SortColumn = null!,
                SortDirection = "asc"
            };

            // act
            var result = testData.ApplySorting(request).ToList();

            // assert
            Assert.Equal(3, result.Count);
            Assert.Equal(3, result[0].Id); 
        }

        // helper class
        private class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
