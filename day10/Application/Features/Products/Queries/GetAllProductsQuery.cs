
using MediatR;
using day10.Domain.Entities;
namespace day10.Application.Features.Products.Queries;

public record GetAllProductsQuery() : IRequest<IEnumerable<Product>>;