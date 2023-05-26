using System.Collections.Generic;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Firebend.AutoCrud.Core.Models.DomainEvents
{
    public class EntityUpdatedDomainEvent<T> : DomainEventBase
        where T : class
    {
        private T _modified;
        private List<Operation<T>> _operations;
        private JsonPatchDocument<T> _patch;

        public T Previous { get; set; }

        public List<Operation<T>> Operations
        {
            get => _operations;
            set
            {
                _operations = value;
                _patch = null;
                _modified = null;
            }
        }

        [JsonIgnore]
        public JsonPatchDocument<T> Patch
            => _patch ??= Operations?.HasValues() ?? false ? new JsonPatchDocument<T>(Operations, new DefaultContractResolver()) : null;

        [JsonIgnore]
        public T Modified => _modified ??= GetModified(Previous, Patch);

        private static T GetModified(T previous,  JsonPatchDocument<T> patchDocument)
        {
            if(patchDocument is null || previous is null)
            {
                return null;
            }

            var clone = previous.Clone();
            patchDocument.ApplyTo(clone);
            return clone;
        }
    }
}
