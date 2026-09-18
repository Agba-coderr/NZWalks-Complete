namespace NZWalks.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
    }

    public abstract class NamedEntity : BaseEntity
    {
        public required string Name { get; set; }
    }
}
