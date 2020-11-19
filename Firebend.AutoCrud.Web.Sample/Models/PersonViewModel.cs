using System;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class PersonViewModel
    {
        public PersonViewModel()
        {

        }

        public PersonViewModel(EfPerson entity)
        {
            Body = new PersonViewModelBase(entity);
        }

        public PersonViewModel(MongoPerson entity)
        {
            Body = new PersonViewModelBase(entity);
        }

        [FromBody]
        public PersonViewModelBase Body { get; set; }
    }

    public class PersonViewModelBase : IEntity<Guid>
    {
        public PersonViewModelBase()
        {
        }

        public PersonViewModelBase(EfPerson person)
        {
            FirstName = person.FirstName;
            LastName = person.LastName;
            Id = person.Id;
            NickName = person.NickName;
            IsDeleted = person.IsDeleted;
            CreatedDate = person.CreatedDate;
            ModifiedDate = person.ModifiedDate;
        }

        public PersonViewModelBase(MongoPerson person)
        {
            FirstName = person.FirstName;
            LastName = person.LastName;
            Id = person.Id;
            NickName = person.LastName;
            IsDeleted = person.IsDeleted;
            CreatedDate = person.CreatedDate;
            ModifiedDate = person.ModifiedDate;
        }

        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string LastName { get; set; }

        [Key]
        public Guid Id { get; set; }

        [StringLength(100)]
        public string NickName { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
