using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Abstractions.Builders;

public abstract class EntityBuilder<TKey, TEntity> : BaseBuilder
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    private Type _entityType;
    private Type _entityKeyType;

    private string _signatureBase;
    public string EntityName { get; set; }

    public Type EntityType => _entityType ??= typeof(TEntity);

    public Type EntityKeyType => _entityKeyType ??= typeof(TKey);

    public Type ExportType { get; set; }

    public override string SignatureBase
    {
        get => _signatureBase ??= $"{EntityType.Name}_{EntityName}";
        set => _signatureBase = value;
    }
}
