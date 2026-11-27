using API.CustomExceptions;
using API.Data;
using API.Dtos;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace APi.Services;

public class ItemService(DataContext _repository) : IItemService
{
    public async Task<ItemCopy> AddCopy(int itemId, CreateItemCopyReq requestData)
    {
        // Nimikkeen pitää olla olemassa. GetById heittää NotFoundExceptionin, jos ei ole.
        await GetById(itemId);

        // Inventaarionumero on yksilöllinen, samoin kuin kategorian nimi
        var codeTaken = await _repository.ItemCopies
            .AnyAsync(c => c.InventoryCode == requestData.InventoryCode);

        if (codeTaken)
        {
            throw new BusinessRuleException($"inventory code {requestData.InventoryCode} already exists");
        }

        var copy = new ItemCopy
        {
            InventoryCode = requestData.InventoryCode,
            ItemId = itemId
        };

        await _repository.ItemCopies.AddAsync(copy);
        await _repository.SaveChangesAsync();

        return copy;
    }

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

    public async Task<Item> GetById(int id)
    {
        var item = await _repository.Items
            .Include(i => i.Category)
            .Include(i => i.Copies)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            throw new NotFoundException($"item {id} not found");
        }

        return item;
    }
}