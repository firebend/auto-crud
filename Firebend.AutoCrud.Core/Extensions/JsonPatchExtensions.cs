using System.Linq;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.Core.Extensions;

public static class JsonPatchExtensions
{
    public static bool TryCopyTo<TFrom, TTo>(this JsonPatchDocument<TFrom> fromPatch, JsonPatchDocument<TTo> toPatch, out string errorMessage)
        where TFrom : class
        where TTo : class, new()
    {
        var result = Result.Success();
        void TestPatch(JsonPatchDocument<TTo> jsonPatchDocument)
        {
            var testEntity = new TTo();
            jsonPatchDocument.ApplyTo(testEntity,
                error =>
                {
                    result.WasSuccessful = false;
                });
        }

        toPatch.Operations.AddRange(
            fromPatch.Operations.Select(o => new Operation<TTo>(o.op, o.path, o.from, o.value)));
        TestPatch(toPatch);
        errorMessage = result.Message;
        return result.WasSuccessful;
    }
}
