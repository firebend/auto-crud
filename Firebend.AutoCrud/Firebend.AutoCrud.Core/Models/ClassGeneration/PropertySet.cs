#region

using System;

#endregion

namespace Firebend.AutoCrud.Core.Models.ClassGeneration
{
    public class PropertySet
    {
        public string Name { get; set; }

        public Type Type { get; set; }

        public object Value { get; set; }

        public bool Override { get; set; }
    }
}