using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class GetPersonViewModel : PersonViewModelBase, IEntityViewModelRead<Guid>, ICustomFieldsEntity<Guid>, IActiveEntity
{
    private static readonly HashSet<string> Ignores = [nameof(CustomFields)];
    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }

    public GetPersonViewModel()
    {

    }

    public GetPersonViewModel(EfPerson entity)
    {
        if (entity == null)
        {
            return;
        }

        entity.CopyPropertiesTo(this, Ignores);

        CustomFields = entity.CustomFields?.Select(x => new CustomFieldsEntity<Guid>(x)).ToList();
    }

    public GetPersonViewModel(MongoTenantPerson entity)
    {
        entity?.CopyPropertiesTo(this);
    }

    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset ModifiedDate { get; set; }
}
