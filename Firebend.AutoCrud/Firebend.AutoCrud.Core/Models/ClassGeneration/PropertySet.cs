using System;

namespace Firebend.AutoCrud.Core.Models.ClassGeneration
{
    public class PropertySet
    {
        public string Name { get; set; }

        public virtual Type Type { get; set; }

        public object Value { get; set; }

        public bool Override { get; set; }
    }

    public class PropertySet<T> : PropertySet
    {
        public override Type Type => typeof(T);
    }
}
