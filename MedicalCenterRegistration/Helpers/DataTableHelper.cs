// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Dynamic.Core; // allows dynamic sorting
using MedicalCenterRegistration.Models.DataTables;

namespace MedicalCenterRegistration.Helpers
{
    public static class DataTableHelper
    {
        public static DataTableRequest GetRequest(Microsoft.AspNetCore.Http.HttpRequest request)
            => DataTableRequest.FromHttpRequest(request);

        public static DataTableResponse<T> CreateResponse<T>(
            DataTableRequest request,
            int totalRecords,
            int filteredRecords,
            IEnumerable<T> data)
        {
            return new DataTableResponse<T>
            {
                Draw = request.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = filteredRecords,
                Data = data
            };
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, DataTableRequest request)
        {
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                string sortExpression = $"{request.SortColumn} {request.SortDirection}";
                query = query.OrderBy(sortExpression);
            }
            return query;
        }
    }
}
