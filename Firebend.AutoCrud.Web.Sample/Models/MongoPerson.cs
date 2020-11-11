namespace Firebend.AutoCrud.Web.Sample.Models
{
    using System;
    using Core.Extensions;
    using Core.Interfaces.Models;

    public class MongoPerson : IEntity<Guid>, IActiveEntity, IModifiedEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public Guid Id { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }

        public MongoPerson()
        {

        }

        public MongoPerson(PersonViewModel viewModel)
        {
            viewModel.CopyPropertiesTo(this);
        }
    }
}
