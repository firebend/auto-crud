using System;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Attributes;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Web.Sample.Models;


public class MongoPerson : IPerson, IEntityDataAuth
{
    public MongoPerson()
    {
    }

    public MongoPerson(CreatePersonViewModel viewModel)
    {
        viewModel?.Body.CopyPropertiesTo(this);
    }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [AutoCrudIgnoreUpdate]
    public string IgnoreMe { get; set; }

    public bool IsDeleted { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
    public string NickName { get; set; }
    public DataAuth DataAuth { get; set; }
}
