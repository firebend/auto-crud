using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class EntityViewModelBase: IEntityDataAuth
{
    public DataAuth DataAuth { get; set; }
}

public class EntityViewModelCreate
{
    public IEntityDataAuth Body { get; set; }
}

public class EntityViewModelRead<TEntity> : EntityViewModelBase, IEntity<Guid>
    where TEntity : IEntity<Guid>, IEntityDataAuth
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }

    public EntityViewModelRead()
    {
    }

    public EntityViewModelRead(TEntity type)
    {
        type?.CopyPropertiesTo(this);
    }
}

public class EntityViewModelExport<T> : EntityViewModelBase, IEntity<Guid>
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }

    public EntityViewModelExport()
    {
    }

    public EntityViewModelExport(T type)
    {
        type?.CopyPropertiesTo(this);
    }
}
