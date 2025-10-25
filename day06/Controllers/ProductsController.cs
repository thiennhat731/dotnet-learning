using day06.Models;
using Microsoft.AspNetCore.Mvc;

namespace day06.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 1500, Category = "Electronics" },
        new Product { Id = 2, Name = "Book", Price = 30, Category = "Stationery" }
    };

    // GET: api/products
    [HttpGet]
    public IActionResult GetAll() => Ok(_products);

    // GET: api/products/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return product == null ? NotFound() : Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public IActionResult Create(Product newProduct)
    {
        newProduct.Id = _products.Max(p => p.Id) + 1;
        _products.Add(newProduct);
        return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
    }

    // PUT: api/products/1
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Product updated)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        product.Name = updated.Name;
        product.Price = updated.Price;
        product.Category = updated.Category;
        return NoContent();
    }

    // DELETE: api/products/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        _products.Remove(product);
        return NoContent();
    }
}
