using System.Security.Claims;
using CollabDoc.Application.Documents.Commands.CreateDocument;
using CollabDoc.Application.Documents.Commands.UpdateDocument;
using CollabDoc.Application.Documents.Commands.DeleteDocument;
using CollabDoc.Application.Documents.Queries.GetAllDocuments;
using CollabDoc.Application.Documents.Queries.GetDocumentById;
using CollabDoc.Application.Documents.Queries.SearchDocuments;
using CollabDoc.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CollabDoc.Domain.Entities;

namespace CollabDoc.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool isDescending = true,
        [FromQuery] string? keyword = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetAllDocumentsQuery(page, pageSize, sortBy, isDescending, keyword, userId);
        return Ok(await _mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(string id)
        => Ok(await _mediator.Send(new GetDocumentByIdQuery(id)));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] DocumentCreateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var created = await _mediator.Send(new CreateDocumentCommand(dto, userId));
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(string id, [FromBody] Document doc)
    {
        await _mediator.Send(new UpdateDocumentCommand(id, doc));
        return Ok(new { message = "Cập nhật thành công." });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        await _mediator.Send(new DeleteDocumentCommand(id));
        return Ok(new { message = "Xóa tài liệu thành công." });
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string query)
        => Ok(await _mediator.Send(new SearchDocumentsQuery(query)));
}
