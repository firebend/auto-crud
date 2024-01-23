using System;
using Humanizer;

namespace Firebend.AutoCrud.Web.Attributes;

public class OpenApiEntityNameAttribute : Attribute
{
    public OpenApiEntityNameAttribute()
    {
    }

    public OpenApiEntityNameAttribute(string name, string plural)
    {
        Name = name;
        Plural = plural;
    }

    public OpenApiEntityNameAttribute(string name) : this(name, name.Pluralize())
    {
    }

    public string Name { get; set; }

    public string Plural { get; set; }
}
