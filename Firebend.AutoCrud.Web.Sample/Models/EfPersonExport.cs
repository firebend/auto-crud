using System;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    using Core.Extensions;

    public class EfPersonExport
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public Guid Id { get; set; }

        public EfPersonExport()
        {

        }

        public EfPersonExport(EfPerson person)
        {
            person.CopyPropertiesTo(this);
        }
    }
}
