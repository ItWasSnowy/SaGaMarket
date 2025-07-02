using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class Tag
{
    public string TagId { get; set; }
    public List<Product> Products { get; set; } = new();
    public Tag() { }
    public Tag (TagDto tagDto) 
    {
        TagId = tagDto.TagId;
    }

}
