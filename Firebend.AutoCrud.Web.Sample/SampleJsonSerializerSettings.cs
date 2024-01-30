using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Firebend.AutoCrud.Web.Sample;

public class SampleJsonSerializerSettings
{
    private static readonly StringEnumConverter StringEnumConverter = new();
    private static readonly CamelCaseExceptDictionaryKeysResolver CamelCaseExceptDictionaryKeysResolver = new();

    public static JsonSerializerSettings Configure(JsonSerializerSettings serializerSettings = null)
    {
        serializerSettings ??= new JsonSerializerSettings();

        serializerSettings.NullValueHandling = NullValueHandling.Ignore;
        serializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
        serializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

        serializerSettings.Converters.Add(StringEnumConverter);
        serializerSettings.ContractResolver = CamelCaseExceptDictionaryKeysResolver;

        serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
        serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        serializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;

        serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        return serializerSettings;
    }
}
