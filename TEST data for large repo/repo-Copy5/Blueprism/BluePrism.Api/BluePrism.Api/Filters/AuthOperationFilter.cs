namespace BluePrism.Api.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Description;
    using Swashbuckle.Swagger;

    public class AuthOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            operation.responses.Add("401", new Response { description = "Unauthorized" });
            operation.responses.Add("403", new Response { description = "Forbidden" });

            if (operation.security == null)
            {
                operation.security = new List<IDictionary<string, IEnumerable<string>>>();
            }

            operation.security.Add(new Dictionary<string, IEnumerable<string>>
            {
                { "OAuth2", Enumerable.Empty<string>() },
                { "Bearer", Enumerable.Empty<string>() }
            });
        }
    }
}
