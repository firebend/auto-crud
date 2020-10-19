using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.Web.Sample.Filtering
{
    public class EfPersonFilter : IEntityFrameworkFullTextExpressionProvider<Guid, EfPerson>
    {
        public Expression<Func<EfPerson, string, bool>> Filter { get; } = (person, s) => person.FirstName.Contains(s) || person.LastName.Contains(s);
        
        public string Test { get; } = nameof(EfPersonFilter);
    }
}