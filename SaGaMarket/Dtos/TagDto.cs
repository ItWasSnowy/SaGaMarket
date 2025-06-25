using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;

public class TagDto
{
    public string TagName { get; set; }

    //Связи ef



    public TagDto() { }
    public TagDto(Tag tag)
    {
        TagName = tag.TagName;
    }
}
