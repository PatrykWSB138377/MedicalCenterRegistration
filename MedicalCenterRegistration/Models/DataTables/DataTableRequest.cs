// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MedicalCenterRegistration.Models.DataTables
{
    public class DataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string SearchValue { get; set; } = string.Empty;
        public string SortColumn { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public List<string> Columns { get; set; } = new();

        public static DataTableRequest FromHttpRequest(HttpRequest request)
        {
            var form = request.Form;

            var columnsData = form
                .Where(x => x.Key.StartsWith("columns["))
                .Where(x => x.Key.EndsWith("][data]"))
                .Select(x => x.Value.ToString())
                .ToList();

            var columnsName = form
                .Where(x => x.Key.StartsWith("columns["))
                .Where(x => x.Key.EndsWith("][name]"))
                .Select(x => x.Value.ToString())
                .ToList();

            int sortColIndex = Convert.ToInt32(form["order[0][column]"].FirstOrDefault() ?? "0");

            string sortColumn = !string.IsNullOrEmpty(columnsName[sortColIndex]) ? columnsName[sortColIndex] : columnsData[sortColIndex];

            return new DataTableRequest
            {
                Draw = Convert.ToInt32(form["draw"].FirstOrDefault() ?? "1"),
                Start = Convert.ToInt32(form["start"].FirstOrDefault() ?? "0"),
                Length = Convert.ToInt32(form["length"].FirstOrDefault() ?? "10"),
                SearchValue = form["search[value]"].FirstOrDefault() ?? string.Empty,
                SortDirection = form["order[0][dir]"].FirstOrDefault() ?? "asc",
                SortColumn = sortColumn,
                Columns = columnsData
            };
        }
    }
}
