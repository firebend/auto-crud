using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch
{
    public interface IJsonPatchDocumentGenerator
    {
        JsonPatchDocument<T> Generate<T>(T a, T b) where T : class;
    }
}