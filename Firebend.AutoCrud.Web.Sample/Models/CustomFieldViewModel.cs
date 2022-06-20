using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CustomFieldViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; }

    public string Value { get; set; }
}

public static class CustomFieldExtensions
{
    public static CustomFieldViewModel ToViewModel(this CustomFieldsEntity<Guid> customField) => customField == null
        ? null
        : new CustomFieldViewModel { Id = customField.Id, Key = customField.Key, Value = customField.Value };

    public static IEnumerable<CustomFieldViewModel>
        ToViewModel(this IEnumerable<CustomFieldsEntity<Guid>> customFields) => customFields == null
        ? new List<CustomFieldViewModel>()
        : customFields.Select(customField =>
            new CustomFieldViewModel { Id = customField.Id, Key = customField.Key, Value = customField.Value });
}
