using System.ComponentModel.DataAnnotations;

namespace Firebend.AutoCrud.CustomFields.Web.Models
{
    public class CustomFieldViewModelCreate
    {
        [MinLength(1)]
        [MaxLength(255)]
        public string Key { get; set; }

        [MinLength(1)]
        [MaxLength(1000)]
        public string Value { get; set; }
    }
}
