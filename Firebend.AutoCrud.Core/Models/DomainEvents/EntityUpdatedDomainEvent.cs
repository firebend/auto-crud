using System.Collections.Generic;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Firebend.AutoCrud.Core.Models.DomainEvents;

public class EntityUpdatedDomainEvent<T> : DomainEventBase
    where T : class
{
    public T Previous { get; set; }

    public List<Operation<T>> Operations
    {
        get;
        set
        {
            field = value;
            Patch = null;
            Modified = null;
        }
    }

    [JsonIgnore]
    public JsonPatchDocument<T> Patch
    {
        get => field ??= Operations?.HasValues() ?? false ? new JsonPatchDocument<T>(Operations, new DefaultContractResolver()) : null;
        private set;
    }

    [JsonIgnore]
    public T Modified
    {
        get => field ??= GetModified(Previous, Patch);
        private set;
    }

    private static T GetModified(T previous, JsonPatchDocument<T> patchDocument)
    {
        if (patchDocument is null || previous is null)
        {
            return null;
        }

        var clone = previous.Clone();
        patchDocument.ApplyTo(clone);
        return clone;
    }
}
