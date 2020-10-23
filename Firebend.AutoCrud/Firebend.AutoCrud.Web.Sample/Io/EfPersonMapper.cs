using System;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.Web.Sample.Io
{
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
            FirstName = person.FirstName;
            LastName = person.LastName;
            Id = person.Id;
        }
    }
    
    public class EfPersonMapper : IEntityExportMapper<EfPerson, EfPersonExport>
    {
        public EfPersonExport Map(EfPerson entity) => new EfPersonExport(entity);
    }
}