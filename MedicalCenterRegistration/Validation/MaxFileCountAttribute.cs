// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MedicalCenterRegistration.Validation
{
    public class MaxFileCountAttribute : ValidationAttribute
    {
        private readonly int _maxFiles;

        public MaxFileCountAttribute(int maxFiles)
        {
            _maxFiles = maxFiles;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var files = value as List<IFormFile>;

            if (files != null && files.Count > _maxFiles)
            {
                return new ValidationResult(ErrorMessage ?? $"Maximum {_maxFiles} files allowed.");
            }

            return ValidationResult.Success;
        }
    }
}
