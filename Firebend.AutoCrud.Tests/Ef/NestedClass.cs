using System;
using System.Collections.Generic;

namespace Firebend.AutoCrud.Tests.Ef;

public class NestedClass
{
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string StringField { get; set; }
    public List<Guid> GuidList { get; set; }
}
