using System.ComponentModel.DataAnnotations;

namespace Firebend.AutoCrud.CustomFields.Web.Models
{
    public class CustomAttributeViewModelCreate
    {
        [MaxLength(250)]
        public string Key { get; set; }

        [MaxLength(250)]
        public string Value { get; set; }
    }
}
