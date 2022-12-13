using System.Reflection;

namespace Firebend.AutoCrud.Core.ObjectMapping
{
    /// <summary>
    /// This class is created for holding source and target
    /// </summary>
    public record PropertyMap(PropertyInfo SourceProperty, PropertyInfo TargetProperty);
}
