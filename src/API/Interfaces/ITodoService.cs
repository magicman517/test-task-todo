using API.Models.Common;
using API.Models.Requests;
using API.Models.Responses;
using ErrorOr;

namespace API.Interfaces;

public interface ITodoService
{
    Task<PaginatedResult<TodoDto>> GetListAsync(
        Guid userId,
        GetTodosQuery query,
        CancellationToken ct = default);

    Task<ErrorOr<TodoDto>> GetByIdAsync(
        Guid userId,
        Guid todoId,
        CancellationToken ct = default);

    Task<ErrorOr<TodoDto>> CreateAsync(
        Guid userId,
        CreateTodoRequest request,
        CancellationToken ct = default);

    Task<ErrorOr<TodoDto>> UpdateAsync(
        Guid userId,
        Guid todoId,
        UpdateTodoRequest request,
        CancellationToken ct = default);

    Task<ErrorOr<Deleted>> DeleteAsync(
        Guid userId,
        Guid todoId,
        CancellationToken ct = default);

    Task<ErrorOr<IReadOnlyList<TodoDto>>> SeedAsync(
        Guid userId,
        int count = 10,
        CancellationToken ct = default);
}
