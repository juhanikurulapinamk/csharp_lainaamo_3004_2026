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

    
    public async Task<Item> Create(CreateItemReq requestData)
    {
        var categoryExists = await _repository.Categories
            .AnyAsync(c => c.Id == requestData.CategoryId);

        if (!categoryExists)
        {
            throw new NotFoundException($"category {requestData.CategoryId} not found");
        }

        // 1. Haetaan ne tagit, jotka ovat jo tietokannassa
        var existingTags = await _repository.Tags
            .Where(tag => requestData.Tags.Contains(tag.TagText))
            .ToListAsync();

        // 2. Selvitetään, mitkä ovat uusia.
        // Except palauttaa jokaisen arvon vain kerran, joten saman tagin
        // antaminen pyynnössä kahdesti ei luo kahta samannimistä tagia.
        var existingTexts = existingTags.Select(t => t.TagText).ToList();
        var newTexts = requestData.Tags.Except(existingTexts).ToList();

        // 3. Luodaan uudet
        var newTags = newTexts.Select(text => new Tag { TagText = text }).ToList();
        await _repository.Tags.AddRangeAsync(newTags);

        // 4. Liitetään sekä vanhat että uudet nimikkeeseen
        var allTags = existingTags.Concat(newTags).ToList();

        var item = new Item
        {
            Name = requestData.Name,
            Description = requestData.Description,
            CategoryId = requestData.CategoryId,
            Tags = allTags
        };

        await _repository.Items.AddAsync(item);

        // Yksi SaveChangesAsync tallentaa sekä uudet tagit,
        // nimikkeen että välitaulurivit — kaikki samassa transaktiossa
        await _repository.SaveChangesAsync();

        return await GetById(item.Id);
    }

    public async Task<IEnumerable<Item>> GetAll()
    {
        
        return await _repository
        .Items
        .Include(i => i.Category)
        .Include(i => i.Copies)
        .Include(i => i.Tags)
        .ToListAsync();
    }

    public async Task<Item> GetById(int id)
    {
        var item = await _repository.Items
            .Include(i => i.Category)
            .Include(i => i.Copies)
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            throw new NotFoundException($"item {id} not found");
        }

        return item;
    }
}