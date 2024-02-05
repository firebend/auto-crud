using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class GetPersonViewModelV2 : PersonViewModelBaseV2, IEntityViewModelRead<Guid>, ICustomFieldsEntity<Guid>
{
    private static readonly HashSet<string> Ignores = [nameof(CustomFields)];
    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }

    public GetPersonViewModelV2()
    {

    }

    public GetPersonViewModelV2(EfPerson entity)
    {
        if (entity == null)
        {
            return;
        }

        entity.CopyPropertiesTo(this, Ignores);

        CustomFields = entity.CustomFields?.Select(x => new CustomFieldsEntity<Guid>(x)).ToList();

        Name = new Name
        {
            First = entity.FirstName,
            Last = entity.LastName,
            NickName = entity.NickName
        };
    }

    public GetPersonViewModelV2(MongoTenantPerson entity)
    {
        entity?.CopyPropertiesTo(this);
        Name = new Name
        {
            First = entity?.FirstName,
            Last = entity?.LastName,
            NickName = entity?.NickName
        };
    }

    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset ModifiedDate { get; set; }
}
