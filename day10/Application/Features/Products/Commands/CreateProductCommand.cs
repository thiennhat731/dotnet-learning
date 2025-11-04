using day10.Domain.Entities;
using day10.Application.Interfaces;
using MediatR;

namespace day10.Application.Features.Products.Commands;

public record CreateProductCommand(string Name, double Price, string Category) : IRequest<Product>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Product>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Category = request.Category
        };

        await _productRepository.CreateAsync(product);
        return product;
    }
}
