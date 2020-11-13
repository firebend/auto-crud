using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Firebend.AutoCrud.Core.Implementations.JsonPatch
{
    public class JsonPatchDocumentDocumentGenerator : IJsonPatchDocumentGenerator
    {
        /// <summary>
        ///     Generates a JsonPatchDocument by comparing two objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a">The original object</param>
        /// <param name="b">The modified object</param>
        /// <returns>The <see cref="JsonPatchDocument" /></returns>
        public JsonPatchDocument<T> Generate<T>(T a, T b)
            where T : class
        {
            var output = new JsonPatchDocument<T>();

            if (ReferenceEquals(a, b))
            {
                return output;
            }

            var jsonSerializer = GetJsonSerializer();

            var originalJson = JObject.FromObject(a, jsonSerializer);
            var modifiedJson = JObject.FromObject(b, jsonSerializer);

            FillJsonPatchValues(originalJson, modifiedJson, output);

            return output;
        }

        public virtual JsonSerializer GetJsonSerializer() => JsonSerializer.CreateDefault();

        /// <summary>
        ///     Fills the json patch values.
        /// </summary>
        /// <param name="originalJson">The original json.</param>
        /// <param name="modifiedJson">The modified json.</param>
        /// <param name="patch">The patch.</param>
        /// <param name="currentPath">The current path.</param>
        private static void FillJsonPatchValues<T>(JObject originalJson,
            JObject modifiedJson,
            JsonPatchDocument<T> patch,
            string currentPath = "/")
            where T : class
        {
            var originalPropertyNames = new HashSet<string>(originalJson.Properties().Select(p => p.Name));
            var modifiedPropertyNames = new HashSet<string>(modifiedJson.Properties().Select(p => p.Name));

            // Remove properties not in modified.
            foreach (var propName in originalPropertyNames.Except(modifiedPropertyNames))
            {
                var path = $"{currentPath}{propName}";

                patch.Operations.Add(new Operation<T>("remove", path, null));
            }

            // Add properties not in original
            foreach (var propName in modifiedPropertyNames.Except(originalPropertyNames))
            {
                var prop = modifiedJson.Property(propName);
                var path = $"{currentPath}{propName}";

                patch.Operations.Add(new Operation<T>("add", path, null, prop.Value));
            }

            // Modify properties that exist in both.
            foreach (var propName in originalPropertyNames.Intersect(modifiedPropertyNames))
            {
                var originalProp = originalJson.Property(propName);
                var modifiedProp = modifiedJson.Property(propName);

                if (originalProp.Value.Type != modifiedProp.Value.Type)
                {
                    var path = $"{currentPath}{propName}";

                    patch.Operations.Add(new Operation<T>("replace", path, null, modifiedProp.Value));
                }
                else if (!string.Equals(originalProp.Value.ToString(Formatting.None), modifiedProp.Value.ToString(Formatting.None)))
                {
                    if (originalProp.Value.Type == JTokenType.Object)
                    {
                        // Recursively fill nested objects.
                        FillJsonPatchValues(originalProp.Value as JObject,
                            modifiedProp.Value as JObject,
                            patch, $"{currentPath}{propName}/");
                    }
                    else if (modifiedProp.Value is JArray modifiedArray && originalProp.Value is JArray originalArray)
                    {
                        var maxOriginalIndex = originalArray.Count - 1;
                        var path = $"{currentPath}{propName}";

                        for (var modifiedIndex = 0; modifiedIndex < modifiedArray.Count; modifiedIndex++)
                        {
                            if (modifiedIndex > maxOriginalIndex)
                            {
                                //add an object to the patch array

                                patch.Operations.Add(new Operation<T>(
                                    "add",
                                    $"{path}/-",
                                    null,
                                    modifiedArray[modifiedIndex]));
                            }
                            else if (modifiedIndex <= maxOriginalIndex)
                            {
                                if (originalArray[modifiedIndex] is JObject originalObject
                                    && modifiedArray[modifiedIndex] is JObject modifiedObject)
                                {
                                    //replace an object from the patch array
                                    FillJsonPatchValues(originalObject,
                                        modifiedObject,
                                        patch,
                                        $"{path}/{modifiedIndex}/");
                                }
                                else if (originalArray[modifiedIndex]?.ToString() != modifiedArray[modifiedIndex]?.ToString())
                                {
                                    patch.Operations.Add(new Operation<T>(
                                        "replace",
                                        $"{path}/{modifiedIndex}",
                                        null,
                                        modifiedArray[modifiedIndex]));
                                }
                            }
                        }

                        var diff = originalArray.Count - modifiedArray.Count;

                        if (diff > 0)
                        {
                            var counter = 0;

                            while (counter < diff)
                            {
                                patch.Operations.Add(new Operation<T>(
                                    "remove",
                                    $"{path}/{maxOriginalIndex - counter}",
                                    null));

                                counter++;
                            }
                        }
                    }
                    else
                    {
                        var path = $"{currentPath}{propName}";

                        // Simple Replace otherwise to make patches idempotent.
                        patch.Operations.Add(new Operation<T>(
                            "replace",
                            path,
                            null,
                            modifiedProp.Value));
                    }
                }
            }
        }
    }
}
