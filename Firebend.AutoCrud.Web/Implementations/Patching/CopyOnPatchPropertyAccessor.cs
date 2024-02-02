using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.Patching;

public class CopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel> : ICopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel>
{
    private readonly ICollection<string> _copyOnPatchPropertyNames;

    public CopyOnPatchPropertyAccessor(List<string> copyOnPatchPropertyNames)
    {
        copyOnPatchPropertyNames ??= new List<string>();
        copyOnPatchPropertyNames.Add(nameof(ICustomFieldsEntity<Guid>.CustomFields));
        _copyOnPatchPropertyNames = copyOnPatchPropertyNames;
    }

    public CopyOnPatchPropertyAccessor()
    {
        _copyOnPatchPropertyNames = [nameof(ICustomFieldsEntity<Guid>.CustomFields)];
    }

    public ICollection<string> GetProperties() => _copyOnPatchPropertyNames;
}
