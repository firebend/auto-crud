using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.Web.Sample.Ordering
{
    public class EfPersonOrder : IEntityDefaultOrderByProvider<Guid, EfPerson>
    {
        public (Expression<Func<EfPerson, object>> func, bool @ascending) OrderBy { get; } = (person => person.LastName, true);
    }
}