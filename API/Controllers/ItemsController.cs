using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using API.Dtos;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using API.CustomExceptions;

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

    
    // POST api/items/5/copies
    [HttpPost("{id}/copies")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<ActionResult<ItemCopyDto>> AddCopy(int id, CreateItemCopyReq req)
    {
        try
        {
            var copy = await _itemService.AddCopy(id, req);

            // Location-header osoittaa nimikkeeseen, johon kappale lisättiin
            return CreatedAtAction(nameof(GetById), new { id },
                new ItemCopyDto { Id = copy.Id, InventoryCode = copy.InventoryCode, ItemId = copy.ItemId });
        }
        catch (NotFoundException e)
        {
            return NotFound(new { title = "item not found", detail = e.Message });
        }
        catch (BusinessRuleException e)
        {
            return Conflict(new { title = "cannot add copy", detail = e.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        try
        {
            var item = await _itemService.GetById(id);

            return Ok(_mapper.Map<ItemDto>(item));
        }
        catch (NotFoundException e)
        {
            return NotFound(new { title = "item not found", detail = e.Message });
        }
    }

    // POST api/items
    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(CreateItemReq req)
    {
        try
        {
            var item = await _itemService.Create(req);

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, _mapper.Map<Item, ItemDto>(item));
        }
        catch (NotFoundException e)
        {
            return NotFound(new { title = "category not found", detail = e.Message });
        }
    }

    

    
}