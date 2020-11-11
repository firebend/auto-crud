using System;
using System.Reflection.Emit;

namespace Firebend.AutoCrud.Core.Models
{
    public class CrudBuilderAttributeModel
    {
        public Type AttributeType { get; set; }

        public CustomAttributeBuilder AttributeBuilder { get; set; }
    }
}
