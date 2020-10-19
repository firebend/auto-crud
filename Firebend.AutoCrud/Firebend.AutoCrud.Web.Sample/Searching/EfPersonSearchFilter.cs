using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.Web.Sample.Searching
{
    public class EfPersonSearchFilter : IEntityFrameworkFullTextExpressionProvider<Guid, EfPerson>
    {
        public Expression<Func<EfPerson, string, bool>> Filter { get; } = (person, s) => person.FirstName.Contains(s) || person.LastName.Contains(s);
    }
}