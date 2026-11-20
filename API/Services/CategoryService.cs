
using API.CustomExceptions;
using API.Data;
using API.Dtos;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    // DataContext injektoidaan konstruktorissa.
    // Se toimii repositoriona, joten erillistä repository-luokkaa ei tarvita.
    public class CategoryService(DataContext _repository) : ICategoryService
    {
        public async Task<IEnumerable<Category>> GetAll()
        {
            // EF Core luo tästä kyselyn: SELECT * FROM Categories
            return await _repository.Categories.ToListAsync();
        }

        public async Task<Category> GetById(int id)
        {
            // FirstOrDefaultAsync palauttaa null, jos riviä ei löydy.
            // FirstAsync heittäisi poikkeuksen.
            // SELECT * FROM Categories WHERE Id = id;
            var category = await _repository.Categories.FirstOrDefaultAsync(c => c.Id == id);

            /*
            
            const markers = [
                {name: 'jokupaikannimi', lat: 1.43, lon: 1.54435}
            ];

            const names = markers.map((m) => {
                return m.name;
            })

            public class Position {
               string Name {get; set;}
               float Lat {get; set;}
               float Long {get; set;}
            }

            
            var makers = List<Position>();
            markers.Add(new Position {Name = "Joku paikka", Lat = 1.45, Lon: 2.156});

            var names = markers.Map(m => m.Name);
            
            */

            if (category == null)
            {
                throw new NotFoundException($"category {id} not found");
            }

            return category;
        }

        public async Task<Category> Create(CreateCategoryReq requestData)
        {

            await EnsureNameIsFree(requestData.Name);

            var category = new Category { Name = requestData.Name };

            // EF Coressa rivi lisätään ensin muutoslistaan
            await _repository.Categories.AddAsync(category);
            // ja tallennetaan vasta SaveChangesAsyncissa
            await _repository.SaveChangesAsync();

            return category;
        }

        public async Task<Category> Update(int id, CreateCategoryReq requestData)
        {
            var category = await GetById(id);

            await EnsureNameIsFree(requestData.Name, id);

            category.Name = requestData.Name;

            // Huomaa: muokattaessa ei tarvita AddAsyncia.
            // EF Core seuraa hakemaansa oliota ja huomaa muutoksen itse.
            await _repository.SaveChangesAsync();

            return category;
        }

        public async Task Delete(int id)
        {
            var category = await GetById(id);
            var hasItems = await _repository.Items.AnyAsync(i => i.CategoryId == id);

            if (hasItems)
            {
                throw new BusinessRuleException("category cannot be deleted while it has items");
            }

            _repository.Categories.Remove(category);
            await _repository.SaveChangesAsync();
        }

        private async Task EnsureNameIsFree(string name, int? exceptId = null)
        {
            var taken = await _repository.Categories
                .AnyAsync(c => c.Name == name && c.Id != exceptId);

            if (taken)
            {
                throw new BusinessRuleException($"category {name} already exists");
            }
        }
    }
}