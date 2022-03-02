namespace BluePrism.Api.Models
{
    using System.Collections.Generic;

    public class UpdateResourceModel
    {
        public IEnumerable<ResourceAttribute> Attributes { get; set; }
    }
}
