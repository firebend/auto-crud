using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Benchmarks.Benchmarks;

public class SampleModel : IEntity<Guid>, ICustomFieldsEntity<Guid>
{
    public Guid Id { get; set; }
    public List<CustomFieldsEntity<Guid>>? CustomFields { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
}
