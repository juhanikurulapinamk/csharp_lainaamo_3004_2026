using API.Data;
using API.Dtos;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace APi.Services;

public class ItemService(DataContext _repository) : IItemService
{
    public Task<Item> Create(CreateItemReq requestData)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Item>> GetAll()
    {
        
        return await _repository
        .Items
        .Include(i => i.Category)
        .Include(i => i.Copies)
        .ToListAsync();
    }

    public Task<Item> GetById(int id)
    {
        throw new NotImplementedException();
    }
}