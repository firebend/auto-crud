using System;

namespace Firebend.AutoCrud.Io.Attributes
{
    public class ExportAttribute : Attribute
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public ExportAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }

        public ExportAttribute()
        {

        }
    }
}
