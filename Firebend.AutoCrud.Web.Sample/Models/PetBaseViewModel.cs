using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class PetBaseViewModel : IEntityViewModelBase, IActiveEntity
{
    [Required]
    [MaxLength(205)]
    public string PetName { get; set; }

    [Required]
    [MaxLength(250)]
    public string PetType { get; set; }

    public DataAuth DataAuth { get; set; }
    public bool IsDeleted { get; set; }
}
