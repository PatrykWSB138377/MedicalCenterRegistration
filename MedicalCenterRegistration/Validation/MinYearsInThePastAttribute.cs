// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Validation
{
    public class MinYearsInThePastAttribute : ValidationAttribute
    {
        private readonly int _minYears;

        public MinYearsInThePastAttribute(int minYears)
        {
            _minYears = minYears;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var minDateInThePast = DateTime.Today.AddYears(-this._minYears);

            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime dateValue)
            {
                if (dateValue > minDateInThePast)
                {
                    return new ValidationResult(ErrorMessage ?? $"Data musi być przed lub równa {minDateInThePast.ToShortDateString()}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
