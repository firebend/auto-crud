using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.ChangeTracking.Models
{
    public class ChangeTrackingSearchRequest : EntitySearchRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string EntityId { get; set; }
    }
}