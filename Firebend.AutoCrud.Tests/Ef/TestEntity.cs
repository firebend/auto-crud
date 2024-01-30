using System;
using System.ComponentModel.DataAnnotations;

namespace Firebend.AutoCrud.Tests.Ef;

public class TestEntity
{
    public Guid Id { get; set; }

    [MaxLength(10000)]
    public string Name { get; set; }
    public NestedClass Nested { get; set; }
}
