using System;

namespace Firebend.AutoCrud.Core.Exceptions;

public class DependencyResolverException : Exception
{
    public DependencyResolverException(string message) : base(message)
    {
    }
}
