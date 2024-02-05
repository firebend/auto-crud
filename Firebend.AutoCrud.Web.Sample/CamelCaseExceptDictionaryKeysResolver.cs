using System;
using Newtonsoft.Json.Serialization;

namespace Firebend.AutoCrud.Web.Sample;

public class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
{
    protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
    {
        var contract = base.CreateDictionaryContract(objectType);

        contract.DictionaryKeyResolver = propertyName => propertyName;

        return contract;
    }
}
