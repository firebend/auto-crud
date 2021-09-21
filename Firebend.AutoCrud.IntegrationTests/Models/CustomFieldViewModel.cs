using System;

namespace Firebend.AutoCrud.IntegrationTests.Models
{
    public class CustomFieldViewModel
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    public class CustomFieldViewModelRead : CustomFieldViewModel
    {
        public Guid Id { get; set; }

        public Guid EntityId { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }
    }
}
