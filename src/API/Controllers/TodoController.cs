using API.Extensions;
using API.Interfaces;
using API.Models.Common;
using API.Models.Requests;
using API.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public sealed class TodoController(ITodoService todoService) : ApiController
{
    [HttpGet]
    [ProducesResponseType<PaginatedResult<TodoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] GetTodosQuery query, CancellationToken ct)
    {
        var result = await todoService.GetListAsync(User.GetUserId(), query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TodoDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await  todoService.GetByIdAsync(User.GetUserId(), id, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [ProducesResponseType<TodoDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateTodoRequest request, CancellationToken ct)
    {
        var result = await todoService.CreateAsync(User.GetUserId(), request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TodoDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoRequest request, CancellationToken ct)
    {
        var result = await todoService.UpdateAsync(User.GetUserId(), id, request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await todoService.DeleteAsync(User.GetUserId(), id, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPost("seed")]
    [ProducesResponseType<IReadOnlyList<TodoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed([FromQuery] int count = 10, CancellationToken ct = default)
    {
        var result = await todoService.SeedAsync(User.GetUserId(), count, ct);
        return result.Match(Ok, Problem);
    }
}
