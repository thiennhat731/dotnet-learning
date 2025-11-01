using day09.Models;
using day09.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace day09.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MongoProductsController : ControllerBase
{
    private readonly IProductMongoRepository _repo;

    public MongoProductsController(IProductMongoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _repo.GetByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product p)
    {
        await _repo.CreateAsync(p);
        return Ok(p);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Product p)
    {
        if (id != p.Id) return BadRequest();
        await _repo.UpdateAsync(p);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
        => Ok(await _repo.SearchByTextAsync(keyword));

    [HttpGet("group")]
    public async Task<IActionResult> Group()
        => Ok(await _repo.GroupByCategoryAsync());
}
