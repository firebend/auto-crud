#region

using System;

#endregion

namespace Firebend.AutoCrud.Web.Attributes
{
    public class OpenApiGroupNameAttribute : Attribute
    {
        public OpenApiGroupNameAttribute(string groupName)
        {
            GroupName = groupName;
        }

        public string GroupName { get; set; }
    }
}