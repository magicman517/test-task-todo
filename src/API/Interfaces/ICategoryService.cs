using API.Models.Requests;
using API.Models.Responses;
using ErrorOr;

namespace API.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<ErrorOr<CategoryDto>> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken ct = default);

    Task<ErrorOr<CategoryDto>> CreateAsync(
        Guid userId,
        CreateCategoryRequest request,
        CancellationToken ct = default);

    Task<ErrorOr<CategoryDto>> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default);

    Task<ErrorOr<Deleted>> DeleteAsync(
        Guid userId,
        Guid id,
        CancellationToken ct = default);

    Task<ErrorOr<IReadOnlyList<CategoryDto>>> SeedAsync(
        Guid userId,
        CancellationToken ct = default);
}
