using System;

namespace Firebend.AutoCrud.Core.Models.ClassGeneration;

public record PropertySet(string Name, Type Type, object Value, bool Override);
