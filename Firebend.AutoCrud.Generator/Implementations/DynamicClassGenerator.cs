using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Firebend.AutoCrud.Core.Models.ClassGeneration;

namespace Firebend.AutoCrud.Generator.Implementations
{
    public class DynamicClassGenerator : IDynamicClassGenerator
    {
        public Type GenerateDynamicClass(Type classType, string typeSignature, List<Type> implementedTypes, Type[] interfaces = null,
            CustomAttributeBuilder[] attributes = null)
        {
            if (!classType.IsClass)
            {
                throw new InvalidCastException("TClass must be a class");
            }

            if (string.IsNullOrWhiteSpace(typeSignature))
            {
                throw new ArgumentNullException(nameof(typeSignature));
            }

            if (interfaces?.Length > 0)
            {
                foreach (var interfaceType in interfaces)
                {
                    if (!interfaceType.IsInterface)
                    {
                        throw new InvalidCastException($"{interfaceType.Name} must be an interface");
                    }
                }
            }

            var tb = GetTypeBuilder(typeSignature, classType, interfaces);
            CreatePassThroughConstructors(tb, classType, implementedTypes);

            if (attributes?.Length > 0)
            {
                foreach (var attribute in attributes)
                {
                    tb.SetCustomAttribute(attribute);
                }
            }

            return tb.CreateType();
        }

        public Type GenerateInterface(Type interfaceType, string typeSignature)
        {
            if (!interfaceType.IsInterface)
            {
                throw new InvalidCastException("TInterface must be an interface");
            }

            if (string.IsNullOrWhiteSpace(typeSignature))
            {
                throw new ArgumentNullException(nameof(typeSignature));
            }

            var tb = GetTypeBuilder(typeSignature, interfaces: new[] { interfaceType }, isInterface: true);

            return tb.CreateType();
        }

        public object ImplementInterface(Type interfaceType, string typeSignature, PropertySet[] properties)
        {
            if (!interfaceType.IsInterface)
            {
                throw new InvalidCastException("TInterface must be an interface");
            }

            if (string.IsNullOrWhiteSpace(typeSignature))
            {
                throw new ArgumentNullException(nameof(typeSignature));
            }

            var tb = GetTypeBuilder(typeSignature, interfaces: new[] { interfaceType });

            tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            var iProperties = interfaceType.GetProperties().Select(x => new PropertySet
            {
                Name = x.Name,
                Type = x.PropertyType,
                Override = true
            });

            properties ??= new PropertySet[0];

            properties = properties
                .Union(iProperties
                    .Where(x => properties
                        .All(y => y.Name != x.Name)))
                .ToArray();

            if (properties.Length > 0)
            {
                foreach (var field in properties)
                {
                    CreateProperty(tb, field);
                }
            }

            var objectType = tb.CreateType();

            if (objectType == null)
            {
                return null;
            }

            var instance = Activator.CreateInstance(objectType);

            if (properties.Length > 0)
            {
                foreach (var field in properties.Where(x => x.Value != null))
                {
                    SetProperty(objectType, instance, field.Name, field.Value);
                }
            }

            return instance;

        }

        private static void CreatePassThroughConstructors(TypeBuilder builder, Type baseType, List<Type> implementedTypes)
        {
            var constructors = baseType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length > 0 && parameters.Last().IsDefined(typeof(ParamArrayAttribute), false))
                    continue;

                var parameterTypes = parameters
                    .Select(pi => implementedTypes.FirstOrDefault(t => pi.ParameterType.IsAssignableFrom(t)) ?? pi.ParameterType)
                    .ToArray();

                var requiredCustomModifiers = parameters.Select(p => p.GetRequiredCustomModifiers()).ToArray();
                var optionalCustomModifiers = parameters.Select(p => p.GetOptionalCustomModifiers()).ToArray();

                var ctor = builder.DefineConstructor(MethodAttributes.Public,
                    constructor.CallingConvention,
                    parameterTypes,
                    requiredCustomModifiers,
                    optionalCustomModifiers);

                for (var i = 0; i < parameters.Length; ++i)
                {
                    var parameter = parameters[i];

                    var parameterBuilder = ctor.DefineParameter(i + 1, parameter.Attributes, parameter.Name);

                    if (((int)parameter.Attributes & (int)ParameterAttributes.HasDefault) != 0)
                    {
                        parameterBuilder.SetConstant(parameter.RawDefaultValue);
                    }

                    foreach (var attribute in BuildCustomAttributes(parameter.GetCustomAttributesData()))
                    {
                        parameterBuilder.SetCustomAttribute(attribute);
                    }
                }

                foreach (var attribute in BuildCustomAttributes(constructor.GetCustomAttributesData()))
                {
                    ctor.SetCustomAttribute(attribute);
                }

                var emitter = ctor.GetILGenerator();

                emitter.Emit(OpCodes.Nop);

                emitter.Emit(OpCodes.Ldarg_0);

                for (var i = 1; i <= parameters.Length; ++i)
                {
                    emitter.Emit(OpCodes.Ldarg, i);
                }

                emitter.Emit(OpCodes.Call, constructor);

                emitter.Emit(OpCodes.Ret);
            }
        }

        private static CustomAttributeBuilder[] BuildCustomAttributes(IEnumerable<CustomAttributeData> customAttributes)
        {
            return customAttributes.Select(attribute =>
            {
                var attributeArgs = attribute.ConstructorArguments
                    .Select(a => a.Value)
                    .ToArray();

                var namedPropertyInfos = attribute.NamedArguments
                    ?.Select(a => a.MemberInfo)
                    .OfType<PropertyInfo>()
                    .ToArray();

                var namedPropertyValues = attribute.NamedArguments
                    ?.Where(a => a.MemberInfo is PropertyInfo)
                    .Select(a => a.TypedValue.Value)
                    .ToArray();

                var namedFieldInfos = attribute.NamedArguments
                    ?.Select(a => a.MemberInfo)
                    .OfType<FieldInfo>()
                    .ToArray();

                var namedFieldValues = attribute.NamedArguments
                    ?.Where(a => a.MemberInfo is FieldInfo)
                    .Select(a => a.TypedValue.Value)
                    .ToArray();

                return new CustomAttributeBuilder(attribute.Constructor,
                    attributeArgs,
                    namedPropertyInfos,
                    namedPropertyValues,
                    namedFieldInfos,
                    namedFieldValues);
            }).ToArray();
        }

        private static TypeBuilder GetTypeBuilder(string typeSignature,
            Type parentType = null,
            Type[] interfaces = null,
            bool isInterface = false)
        {
            var typeAttributes = TypeAttributes.Public | TypeAttributes.AutoLayout;

            if (isInterface)
            {
                typeAttributes |= TypeAttributes.Interface | TypeAttributes.Abstract;
            }
            else
            {
                typeAttributes |= TypeAttributes.Class |
                                  TypeAttributes.AutoClass |
                                  TypeAttributes.AnsiClass |
                                  TypeAttributes.BeforeFieldInit;
            }

            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Firebend.AutoCrud.Dynamic");
            var tb = moduleBuilder.DefineType(
                typeSignature,
                typeAttributes,
                parentType,
                interfaces);

            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, PropertySet propertySet)
        {
            var propertyBuilder = tb.DefineProperty(propertySet.Name, PropertyAttributes.HasDefault, propertySet.Type, null);
            var getterAttributes = MethodAttributes.Public |
                                   MethodAttributes.SpecialName |
                                   MethodAttributes.HideBySig;

            if (propertySet.Override)
            {
                getterAttributes |= MethodAttributes.Virtual;
            }

            var getPropertyMethodBuilder =
                tb.DefineMethod(
                    $"get_{propertySet.Name}",
                    getterAttributes,
                    propertySet.Type,
                    Type.EmptyTypes);

            var fieldBuilder = tb.DefineField($"_{propertySet.Name}", propertySet.Type, FieldAttributes.Private);

            var getMethodBody = getPropertyMethodBuilder.GetILGenerator();
            getMethodBody.Emit(OpCodes.Ldarg_0);
            getMethodBody.Emit(OpCodes.Ldfld, fieldBuilder);
            getMethodBody.Emit(OpCodes.Ret);

            if (propertySet.Override && tb.BaseType != null)
            {
                var baseMethod = tb.BaseType.GetMethod($"get_{propertySet.Name}", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (baseMethod != null)
                {
                    tb.DefineMethodOverride(getPropertyMethodBuilder, baseMethod);
                }
            }

            propertyBuilder.SetGetMethod(getPropertyMethodBuilder);

            var setPropertyMethodBuilder =
                tb.DefineMethod($"set_{propertySet.Name}",
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig |
                    MethodAttributes.Public,
                    null,
                    new[] { propertySet.Type });

            var setMethodBody = setPropertyMethodBuilder.GetILGenerator();
            var modifyProperty = setMethodBody.DefineLabel();
            var exitSet = setMethodBody.DefineLabel();

            setMethodBody.MarkLabel(modifyProperty);
            setMethodBody.Emit(OpCodes.Ldarg_0);
            setMethodBody.Emit(OpCodes.Ldarg_1);
            setMethodBody.Emit(OpCodes.Stfld, fieldBuilder);

            setMethodBody.Emit(OpCodes.Nop);
            setMethodBody.MarkLabel(exitSet);
            setMethodBody.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setPropertyMethodBuilder);
        }

        private static void SetProperty(Type objectType, object instance, string fieldName, object fieldValue)
        {
            var property = objectType.GetProperty(fieldName);

            property?.SetValue(instance, fieldValue);
        }
    }
}
