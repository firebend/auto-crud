namespace Firebend.AutoCrud.Web.Sample.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class PersonViewModel
    {
        [StringLength(250)]
        public string FirstName { get; set; }

        [StringLength(250)]
        public string LastName { get; set; }

        [Key] public Guid Id { get; set; }

        [StringLength(100)]
        public string NickName { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }

        public PersonViewModel()
        {

        }

        public PersonViewModel(EfPerson person)
        {
            FirstName = person.FirstName;
            LastName = person.LastName;
            Id = person.Id;
            NickName = person.LastName;
            IsDeleted = person.IsDeleted;
            CreatedDate = person.CreatedDate;
            ModifiedDate = person.ModifiedDate;
        }

        public PersonViewModel(MongoPerson person)
        {
            FirstName = person.FirstName;
            LastName = person.LastName;
            Id = person.Id;
            NickName = person.LastName;
        }
    }
}
