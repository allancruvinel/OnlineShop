using Microsoft.AspNetCore.Mvc;

namespace Stock.Controllers;
[ApiController]
[Route("[controller]")]
public class ItemController(ItemData _itemData, ILogger<ItemController> logger) : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpGet]
    public List<Item> Get()
    {
        var result = _itemData.Item.ToList();
        return result;
    }
}
