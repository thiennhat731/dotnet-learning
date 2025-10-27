using day07.Models;
using day07.Repositories;

namespace day07.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public IEnumerable<Product> GetAll() => _repo.GetAll();

    public Product? GetById(int id) => _repo.GetById(id);

    public void Create(Product product) => _repo.Add(product);

    public void Update(Product product) => _repo.Update(product);

    public void Delete(int id) => _repo.Delete(id);
}
