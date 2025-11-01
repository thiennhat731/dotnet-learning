using day09.Models;

namespace day09.Repositories;

public interface IProductMongoRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(string id);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(string id);
    Task<IEnumerable<Product>> SearchByTextAsync(string keyword);
    Task<IEnumerable<dynamic>> GroupByCategoryAsync();
}
