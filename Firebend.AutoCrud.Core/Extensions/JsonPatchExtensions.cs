using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Entities;
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

    public static bool HasOperationWithPath(this JsonPatchDocument document, string path)
        => document?.Operations is not null &&
           document.Operations.Any(x =>
               x.path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

    public static bool IsOnlyModifiedEntityPatch(this JsonPatchDocument document)
        => document?.Operations is not null &&
        document.Operations.Count <= 2 &&
        (document.HasOperationWithPath($"/{nameof(IModifiedEntity.CreatedDate)}") ||
         document.HasOperationWithPath($"/{nameof(IModifiedEntity.ModifiedDate)}"));
}
