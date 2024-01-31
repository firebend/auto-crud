using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Core.ObjectMapping;

public record ObjectMapperContext(
    Type SourceType,
    Type TargetType,
    HashSet<string> PropertiesToIgnore,
    HashSet<string> PropertiesToInclude,
    bool IncludeObjects)
{
    private string _key;
    public string Key => _key ??= GetMapKey();
    private string GetMapKey()
    {
        List<string> keys = ["ObjectMapper", SourceType.FullName, TargetType.FullName];

        if (!PropertiesToIgnore.IsEmpty())
        {
            keys.Add("propertiesToIgnore");
            keys.AddRange(PropertiesToIgnore);
        }

        if (!PropertiesToInclude.IsEmpty())
        {
            keys.Add("propertiesToInclude");
            keys.AddRange(PropertiesToInclude);
        }

        keys.Add("includeObject");
        keys.Add(IncludeObjects.ToString());

        return string.Join('_', keys);
    }
}
