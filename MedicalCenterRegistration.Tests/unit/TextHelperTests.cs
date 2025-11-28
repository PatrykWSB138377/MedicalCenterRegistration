using MedicalCenterRegistration.Helpers;

namespace MedicalCenterRegistration.Tests
{
    public class TextHelperTests
    {
        [Theory]
        [InlineData(1, "wizyta", "wizyty", "wizyt", "wizyta")]
        [InlineData(2, "wizyta", "wizyty", "wizyt", "wizyty")]
        [InlineData(3, "wizyta", "wizyty", "wizyt", "wizyty")]
        [InlineData(4, "wizyta", "wizyty", "wizyt", "wizyty")]
        [InlineData(5, "wizyta", "wizyty", "wizyt", "wizyt")]
        [InlineData(11, "wizyta", "wizyty", "wizyt", "wizyt")]
        [InlineData(12, "wizyta", "wizyty", "wizyt", "wizyt")]
        [InlineData(13, "wizyta", "wizyty", "wizyt", "wizyt")]

        public void Pluralize_ReturnsCorrectForm(int value, string singular, string few, string many, string expected)
        {
            // act
            var result = TextHelper.Pluralize(value, singular, few, many);

            // assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Pluralize_WithNullMany_UsesFewForm()
        {
            // arrange
            int value = 5;
            string singular = "item";
            string few = "items";

            // act
            var result = TextHelper.Pluralize(value, singular, few, null);

            // assert
            Assert.Equal(few, result);
        }

        [Theory]
        [InlineData(0, "wizyta", "wizyty", "wizyt", "wizyt")]
        public void Pluralize_WithZero_ReturnsManyForm(int value, string singular, string few, string many, string expected)
        {
            // act
            var result = TextHelper.Pluralize(value, singular, few, many);

            // assert
            Assert.Equal(expected, result);
        }
    }
}
