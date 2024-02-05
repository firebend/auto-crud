using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Models;

public interface IEfCustomFieldsModel<TKey>
    where TKey : struct
{
    CustomFieldsEntity<TKey> ToCustomFields();
}
