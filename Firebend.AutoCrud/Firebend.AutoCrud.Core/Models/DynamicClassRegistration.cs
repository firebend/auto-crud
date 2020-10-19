using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Models.ClassGeneration;

namespace Firebend.AutoCrud.Core.Models
{
    public class DynamicClassRegistration
    {
        public string Signature { get; set; }
        
        public IEnumerable<PropertySet> Properties { get; set; }
        
        public Type Interface { get; set; }
    }
}