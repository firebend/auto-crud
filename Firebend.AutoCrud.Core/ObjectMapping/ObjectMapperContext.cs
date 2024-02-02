using System;
using System.Collections.Generic;
using System.Text;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Core.ObjectMapping;

public record ObjectMapperContext(
    Type SourceType,
    Type TargetType,
    ICollection<string> PropertiesToIgnore,
    ICollection<string> PropertiesToInclude,
    bool IncludeObjects)
{
    private string _key;
    public string Key => _key ??= GetMapKey();
    private string GetMapKey() => GetKeyUsingStringBuilder();
    private string GetKeyUsingStringBuilder()
    {
        var sb = new StringBuilder();
        sb.Append(SourceType.FullName);
        sb.Append(TargetType.FullName);

        if (PropertiesToIgnore.HasValues())
        {
            sb.Append('i');

            foreach (var s in PropertiesToIgnore!)
            {
                sb.Append(s);
            }
        }

        if (PropertiesToInclude.HasValues())
        {
            sb.Append('g');

            foreach (var s in PropertiesToInclude!)
            {
                sb.Append(s);
            }
        }

        sb.Append('o');
        sb.Append(IncludeObjects);
        var built = sb.ToString();
        sb.Clear();
        return built;
    }
}
