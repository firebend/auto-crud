using System;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class EfPersonExport
    {
        public EfPersonExport()
        {
        }

        public EfPersonExport(EfPerson person)
        {
            person.CopyPropertiesTo(this);
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public Guid Id { get; set; }
    }
}
