using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.Core.Extensions;

public static class JsonPatchExtensions
{
    public static bool ValidatePatchModel<T>(this JsonPatchDocument<T> patchDocument,
        out List<ValidationResult> results) where T : class, new()
    {
        var testType = new T();
        patchDocument.ApplyTo(testType);
        var context = new ValidationContext(testType, null, null);
        var allResults = new List<ValidationResult>();

        Validator.TryValidateObject(testType, context, allResults, true);
        results =
            allResults.Where(x =>
                {
                    var errorPath = string.Join('/', x.MemberNames);
                    return patchDocument.Operations.Any(o =>
                        o.path.TrimStart('/').Equals(errorPath, StringComparison.InvariantCultureIgnoreCase));
                })
                .ToList();
        return results.IsEmpty();
    }

    public static bool TryCopyTo<TFrom, TTo>(this JsonPatchDocument<TFrom> fromPatch, JsonPatchDocument<TTo> toPatch,
        out string errorMessage)
        where TFrom : class
        where TTo : class, new()
    {
        var result = Result.Success();

        toPatch.Operations.AddRange(
            fromPatch.Operations.Select(o => new Operation<TTo>(o.op, o.path, o.from, o.value)));
        TestPatch(toPatch);
        errorMessage = result.Message;
        return result.WasSuccessful;

        void TestPatch(JsonPatchDocument<TTo> jsonPatchDocument)
        {
            var testEntity = new TTo();
            jsonPatchDocument.ApplyTo(testEntity,
                _ => result.WasSuccessful = false);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the patch document contains only the CreatedDate or ModifiedDate properties.
    /// </summary>
    /// <param name="document">
    /// The document.
    /// </param>
    /// <returns>
    /// <c>true</c> if the patch document contains only the CreatedDate or ModifiedDate properties; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsOnlyModifiedEntityPatch(this IJsonPatchDocument document) =>
        (document?.GetOperations() ?? []).All(x =>
            x.path.EqualsIgnoreCaseAndWhitespace($"/{nameof(IModifiedEntity.CreatedDate)}") ||
            x.path.EqualsIgnoreCaseAndWhitespace($"/{nameof(IModifiedEntity.ModifiedDate)}"));
}
