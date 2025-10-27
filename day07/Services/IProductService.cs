using day07.Models;

namespace day07.Services;

public interface IProductService
{
    IEnumerable<Product> GetAll();
    Product? GetById(int id);
    void Create(Product product);
    void Update(Product product);
    void Delete(int id);
}
