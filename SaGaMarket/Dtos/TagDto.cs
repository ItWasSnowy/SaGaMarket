using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;

public class TagDto
{
    public string TagId { get; set; }

    //Связи ef



    public TagDto() { }
    public TagDto(Tag tag)
    {
        TagId = tag.TagId;
    }
}
