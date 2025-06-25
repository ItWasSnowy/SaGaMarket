using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class Tag
{
    public string TagName { get; set; }

    // Navigation properties
    public List<Product> Products { get; set; } = new();


    public Tag() { }
    public Tag (TagDto tagDto) 
    {
        TagName = tagDto.TagName;
    }

}
