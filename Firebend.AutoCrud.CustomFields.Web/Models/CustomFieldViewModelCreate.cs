using System.ComponentModel.DataAnnotations;

namespace Firebend.AutoCrud.CustomFields.Web.Models;

public class CustomFieldViewModelCreate
{
    [MinLength(1)]
    [MaxLength(250)]
    public string Key { get; set; }

    [MinLength(1)]
    [MaxLength(250)]
    public string Value { get; set; }
}
