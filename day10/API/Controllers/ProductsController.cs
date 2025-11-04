using day10.Application.Features.Products.Commands;
using day10.Application.Features.Products.Queries;
using day10.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _mediator.Send(new GetAllProductsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
        => Ok(await _mediator.Send(new CreateProductCommand(product.Name, product.Price, product.Category)));
}
