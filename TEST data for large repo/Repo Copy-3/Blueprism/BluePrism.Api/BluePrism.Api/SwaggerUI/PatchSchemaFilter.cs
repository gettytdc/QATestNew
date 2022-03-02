namespace BluePrism.Api.SwaggerUI
{
    using System;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.Swagger;

    public class PatchSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JsonPatchDocument<>))
            {
                var array = new JArray
                {
                    new JObject {["op"] = "add", ["path"] = "/someArrayProperty", ["value"] = new JArray {"Item1", "Item2"} },
                    new JObject {["op"] = "add", ["path"] = "/someProperty", ["value"] = "someValue"},
                    new JObject {["op"] = "copy", ["from"] = "/sourceProperty", ["path"] = "/destinationProperty"},
                    new JObject {["op"] = "move", ["from"] = "/sourceProperty", ["path"] = "/destinationProperty"},
                    new JObject {["op"] = "remove", ["path"] = "/someProperty"},
                    new JObject {["op"] = "replace", ["path"] = "/someProperty", ["value"] = "someValue"},
                };

                schema.example = array;
            }
        }
    }
}
