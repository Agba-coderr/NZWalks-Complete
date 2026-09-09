using NZWalks.Domain.Common;

namespace NZWalks.Domain.Entities
{
    public class Region : NamedEntity
    {
        // Inherits Id and Name from NamedEntity

        public required string Code { get; set; }

        public string? RegionImageUrl { get; set; }
    }
}
