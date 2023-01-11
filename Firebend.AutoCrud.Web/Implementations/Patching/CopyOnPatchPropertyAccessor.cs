using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.Patching;

public class CopyOnPatchPropertyAccessor<TEntity, TViewModel> : ICopyOnPatchPropertyAccessor<TEntity, TViewModel>
{
    private readonly string[] _copyOnPatchPropertyNames;

    public CopyOnPatchPropertyAccessor(List<string> copyOnPatchPropertyNames)
    {
        copyOnPatchPropertyNames ??= new List<string>();
        copyOnPatchPropertyNames.Add(nameof(ICustomFieldsEntity<Guid>.CustomFields));
        _copyOnPatchPropertyNames = copyOnPatchPropertyNames.ToArray();
    }

    public CopyOnPatchPropertyAccessor()
    {
        _copyOnPatchPropertyNames = new[] { nameof(ICustomFieldsEntity<Guid>.CustomFields) };
    }

    public string[] GetProperties() => _copyOnPatchPropertyNames;
}
