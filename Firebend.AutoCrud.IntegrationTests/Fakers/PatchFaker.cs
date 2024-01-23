using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

namespace Firebend.AutoCrud.IntegrationTests.Fakers;

public static class PatchFaker
{
    public static JsonPatchDocument MakeReplacePatch<T, TProp>(Expression<Func<T, TProp>> expression, TProp value)
        where T : class
    {
        var doc = new JsonPatchDocument<T>();
        doc.Replace(expression, value);
        var patch = new JsonPatchDocument(doc.Operations.Cast<Operation>().ToList(), new DefaultContractResolver());
        return patch;
    }
}
