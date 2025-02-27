using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Models.ClassGeneration;

namespace Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;

public interface IDynamicClassGenerator : IDisposable
{
    public Type GenerateDynamicClass(Type classType,
        string typeSignature,
        List<Type> implementedTypes,
        Type[] interfaces = null,
        CustomAttributeBuilder[] attributes = null);

    public Type GenerateInterface(Type interfaceType, string typeSignature);

    public object ImplementInterface(Type interfaceType, string typeSignature, PropertySet[] properties);
}
