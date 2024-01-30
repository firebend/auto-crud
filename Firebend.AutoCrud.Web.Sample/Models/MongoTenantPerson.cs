using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class MongoTenantPerson : MongoPerson, ITenantEntity<int>, ICustomFieldsEntity<Guid>
{
    public int TenantId { get; set; }
    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }
}
