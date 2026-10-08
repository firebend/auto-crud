using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Abstractions.Builders;

public abstract class EntityBuilder<TKey, TEntity> : BaseBuilder
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    private string _signatureBase;
    public string EntityName { get; set; }

    public Type EntityType => field ??= typeof(TEntity);

    public Type EntityKeyType => field ??= typeof(TKey);

    public Type ExportType { get; set; }

    public override string SignatureBase
    {
        get => _signatureBase ??= $"{EntityType.Name}_{EntityName}";
        set => _signatureBase = value;
    }

    protected EntityBuilder(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }
}
