using day07.Models;
using day07.Services;
using Microsoft.AspNetCore.Mvc;

namespace day07.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var product = _service.GetById(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        _service.Create(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Product product)
    {
        if (id != product.Id) return BadRequest();
        _service.Update(product);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}
