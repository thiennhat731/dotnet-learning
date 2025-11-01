using day09.Data;
using day09.Models;
using MongoDB.Driver;

namespace day09.Repositories;

public class ProductMongoRepository : IProductMongoRepository
{
    private readonly MongoDbContext _context;

    public ProductMongoRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await _context.Products.Find(_ => true).ToListAsync();


    public async Task<Product?> GetByIdAsync(string id) =>
        await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Product product) =>
        await _context.Products.InsertOneAsync(product);

    public async Task UpdateAsync(Product product) =>
        await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);

    public async Task DeleteAsync(string id) =>
        await _context.Products.DeleteOneAsync(p => p.Id == id);

    // Text search
    public async Task<IEnumerable<Product>> SearchByTextAsync(string keyword)
    {
        var filter = Builders<Product>.Filter.Text(keyword);
        return await _context.Products.Find(filter).ToListAsync();
    }

    // Aggregation group by Category
    public async Task<IEnumerable<dynamic>> GroupByCategoryAsync()
    {
        var group = await _context.Products.Aggregate()
            .Group(p => p.Category, g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();
        return group;
    }
}
