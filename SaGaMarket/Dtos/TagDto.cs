using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;

public class TagDto
{
    public string TagId { get; set; }

    public TagDto() { }
    public TagDto(Tag tag)
    {
        TagId = tag.TagId;
    }
}
