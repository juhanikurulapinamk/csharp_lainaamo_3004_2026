using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using API.Dtos;
using API.Models;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController(IItemService _itemService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
    {
        var items = await _itemService.GetAll();
        
        var dtos = items.Select(i => new ItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Description = i.Description,
            // i.Category on null, koska sitä ei ladattu!
            CategoryName = i.Category!.Name,
            Category = new CategoryDto
            {
                Id = i.Category!.Id,
                Name = i.Category!.Name
            },
            CopyCount = i.Copies.Count
        });

        return Ok(dtos);
    }
}