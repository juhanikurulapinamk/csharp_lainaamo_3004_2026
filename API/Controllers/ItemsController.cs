using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using API.Dtos;
using API.Models;
using AutoMapper;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController(IItemService _itemService, IMapper _mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
    {
        var items = await _itemService.GetAll();
        
        var dtos = _mapper.Map<IEnumerable<ItemDto>>(items);

        return Ok(dtos);
    }
}