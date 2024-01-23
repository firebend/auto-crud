using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Models;

public class DefaultCreateUpdateViewModel<TKey, TEntity> : IViewModelWithBody<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    [Required]
    [FromBody]
    public TEntity Body { get; set; }
}
