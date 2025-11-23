// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Helpers;
public static class DebugLogger
{
    private static void Log(ILogger logger, string message)
    {
        if (logger != null)
        {
            logger.LogInformation(message);
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    public static void LogList<T>(IEnumerable<T> list, string title = null, ILogger logger = null)
    {
        if (!string.IsNullOrEmpty(title))
            Log(logger, $"\n=== {title} ===");

        if (list == null)
        {
            Log(logger, "List is null");
            return;
        }

        int i = 1;
        foreach (var item in list)
        {
            Log(logger, $"\n-- Item #{i++} --");
            Log(logger, ObjectToString(item));
        }

        Log(logger, $"\nTotal items: {list.Count()}");
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
