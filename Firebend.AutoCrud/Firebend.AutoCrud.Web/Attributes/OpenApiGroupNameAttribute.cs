using System;

namespace Firebend.AutoCrud.Web.Attributes
{
    public class OpenApiGroupNameAttribute : Attribute
    {
        public string GroupName { get; set; }

        public OpenApiGroupNameAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}