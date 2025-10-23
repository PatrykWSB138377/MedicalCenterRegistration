// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalCenterRegistration.Enums
{
    public class EnumHelper
    {
        public static string GetDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())[0]
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }

        public static List<SelectListItem> GetSelectList<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.GetType()
                            .GetMember(e.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName() ?? e.ToString()
                })
                .ToList();
        }
    }
}
