// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Helpers;
public static class DebugLogger
{
    public static void LogList<T>(IEnumerable<T> list, string title = null)
    {
        if (!string.IsNullOrEmpty(title))
            Console.WriteLine($"\n=== {title} ===");

        if (list == null)
        {
            Console.WriteLine("List is null");
            return;
        }

        int i = 1;
        foreach (var item in list)
        {
            Console.WriteLine($"\n-- Item #{i++} --");
            Console.WriteLine(ObjectToString(item));
        }

        Console.WriteLine($"\nTotal items: {list.Count()}");
    }

    private static string ObjectToString(object obj)
    {
        if (obj == null) return "null";

        var props = obj.GetType().GetProperties();
        return string.Join(Environment.NewLine, props.Select(p =>
        {
            var value = p.GetValue(obj);
            return $"{p.Name}: {(value ?? "null")}";
        }));
    }
}
