// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

public class DateNotInPastAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime dateValue)
        {
            if (dateValue < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "The date cannot be in the past.");
            }
        }

        return ValidationResult.Success;
    }
}
