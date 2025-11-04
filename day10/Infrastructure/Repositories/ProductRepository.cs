using day10.Domain.Entities;
using day10.Application.Interfaces;

namespace day10.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 1500, Category = "Electronics" },
        new Product { Id = 2, Name = "Book", Price = 30, Category = "Stationery" }
    };

    public Task<IEnumerable<Product>> GetAllAsync()
        => Task.FromResult<IEnumerable<Product>>(_products);

    public Task<Product?> GetByIdAsync(int id)
        => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task CreateAsync(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        return Task.CompletedTask;
    }


    public Task UpdateAsync(Product product)
    {
        var existing = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existing is not null)
        {
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Category = product.Category;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is not null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }
}
