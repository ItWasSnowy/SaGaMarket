using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Interfaces;

public class ITaggedObject
{
    public List<Tag> Tags { get; set; }
}
