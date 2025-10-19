// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Helpers
{
    public static class TextHelper
    {
        public static string Pluralize(int value, string singular, string few, string? many)
        {
            int absVal = Math.Abs(value);
            int lastDigit = absVal % 10;
            int lastTwoDigits = absVal % 100;

            if (absVal == 1)
                return singular;

            if (lastDigit >= 2 && lastDigit <= 4 && (lastTwoDigits < 12 || lastTwoDigits > 14))
                return few;

            return many ?? few;
        }
    }
}
