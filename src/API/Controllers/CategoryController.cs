using API.Extensions;
using API.Interfaces;
using API.Models.Requests;
using API.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public sealed class CategoryController(ICategoryService categoryService) : ApiController
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await categoryService.GetAllAsync(User.GetUserId(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await categoryService.GetByIdAsync(User.GetUserId(), id, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await categoryService.CreateAsync(User.GetUserId(), request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await categoryService.UpdateAsync(User.GetUserId(), id, request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await categoryService.DeleteAsync(User.GetUserId(), id, ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPost("seed")]
    [ProducesResponseType<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        var result = await categoryService.SeedAsync(User.GetUserId(), ct);
        return result.Match(Ok, Problem);
    }
}
