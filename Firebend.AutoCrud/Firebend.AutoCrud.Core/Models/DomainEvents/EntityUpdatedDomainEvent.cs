using Firebend.AutoCrud.Core.Extensions;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Models.DomainEvents
{
    public class EntityUpdatedDomainEvent<T> : DomainEventBase where T : class
    {
        private T _modified;

        private JsonPatchDocument<T> _patch;

        public T Previous { get; set; }

        public JsonPatchDocument<T> Patch
        {
            get => _patch;
            set
            {
                _patch = value;
                _modified = null;
            }
        }

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