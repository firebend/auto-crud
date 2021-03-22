using System.Collections.Generic;
using Firebend.AutoCrud.Core.Extensions;
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
        private string _operationsJson;
        private List<Operation<T>> _operations;
        private JsonPatchDocument<T> _patch;

        public T Previous { get; set; }

        public string OperationsJson
        {
            get => _operationsJson;
            set
            {
                _operationsJson = value;
                _operations = null;
                _patch = null;
            }
        }

        public List<Operation<T>> Operations => _operations ??=
            JsonConvert.DeserializeObject(OperationsJson, typeof(List<Operation<T>>),
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All}) as List<Operation<T>>;

        public JsonPatchDocument<T> Patch => _patch ??= new JsonPatchDocument<T>(Operations, new DefaultContractResolver());

        public T Modified
        {
            get
            {
                if (_modified != null)
                {
                    return _modified;
                }

                var clone = Previous.Clone();
                Patch.ApplyTo(clone);
                _modified = clone;
                return _modified;
            }
        }
    }
}
