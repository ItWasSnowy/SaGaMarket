using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class Tag
{
    public string TagName { get; set; }




    public Tag() { }
    public Tag (TagDto tagDto) 
    {
        TagName = tagDto.TagName;
    }

}
