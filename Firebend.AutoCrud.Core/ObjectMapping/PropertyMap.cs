using System.Reflection;

namespace Firebend.AutoCrud.Core.ObjectMapping
{
    /// <summary>
    /// This class is created for holding source and target 
    /// </summary>
    public class PropertyMap
    {
        public PropertyInfo SourceProperty { get; set; }
        public PropertyInfo TargetProperty { get; set; }
    }
}
