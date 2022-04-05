using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Extensions;

public static class SearchRequestExtensions
{

    public static ModelStateResult<TSearch> ValidateSearchRequest<TSearch>(this TSearch searchRequest, int? maxSize)
        where TSearch : IEntitySearchRequest
    {
        var modelState = new ModelStateResult<TSearch> { WasSuccessful = true };
        if (searchRequest == null)
        {
            modelState.AddError(nameof(searchRequest), "Search parameters are required.");
            return modelState;
        }

        if (!searchRequest.PageNumber.HasValue || searchRequest.PageNumber.Value < 1)
        {
            modelState.AddError(nameof(searchRequest.PageNumber), "Page number must be greater then 0.");
            return modelState;
        }

        var pageSize = maxSize ?? 100;

        if (!searchRequest.PageSize.GetValueOrDefault().IsBetween(1, pageSize))
        {
            modelState.AddError(nameof(searchRequest.PageSize), $"Page size must be between 1 and {pageSize}.");
            return modelState;
        }

        if (searchRequest is IModifiedEntitySearchRequest modifiedEntitySearchRequest)
        {
            if (modifiedEntitySearchRequest.CreatedStartDate.HasValue
                && modifiedEntitySearchRequest.ModifiedStartDate.HasValue
                && modifiedEntitySearchRequest.CreatedStartDate.Value > modifiedEntitySearchRequest.ModifiedStartDate.Value)
            {
                modelState.AddError(nameof(IModifiedEntitySearchRequest.CreatedStartDate), "Created date cannot be after modified date.");
                return modelState;
            }

            if (modifiedEntitySearchRequest.CreatedStartDate.HasValue
                && modifiedEntitySearchRequest.CreatedEndDate.HasValue
                && modifiedEntitySearchRequest.CreatedStartDate.Value > modifiedEntitySearchRequest.CreatedEndDate.Value)
            {
                modelState.AddError(nameof(IModifiedEntitySearchRequest.CreatedStartDate), "Created start date must be before end date.");
                return modelState;
            }

            if (modifiedEntitySearchRequest.ModifiedEndDate.HasValue
                && modifiedEntitySearchRequest.ModifiedStartDate.HasValue
                && modifiedEntitySearchRequest.ModifiedStartDate.Value > modifiedEntitySearchRequest.ModifiedEndDate.Value)
            {
                modelState.AddError(nameof(IModifiedEntitySearchRequest.ModifiedStartDate), "Modified start date must be before end date.");
                return modelState;
            }
        }
        return modelState;
    }
}
