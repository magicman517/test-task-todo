using API.Interfaces;
using API.Models.Requests;
using API.Models.Responses;
using API.Persistence;
using API.Persistence.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public sealed class CategoryService(AppDbContext dbContext) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(CategoryDto.Projection)
            .ToListAsync(ct);
    }

    public async Task<ErrorOr<CategoryDto>> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken ct = default)
    {
        var category = await dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Id == id)
            .Select(CategoryDto.Projection)
            .FirstOrDefaultAsync(ct);

        return category is null
            ? Error.NotFound("Category.NotFound", "Category not found")
            : category;
    }

    public async Task<ErrorOr<CategoryDto>> CreateAsync(
        Guid userId,
        CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        var normalizedName = request.Name.Trim();

        var exists = await dbContext.Categories
            .AnyAsync(x => x.UserId == userId && x.Name.ToLower() == normalizedName.ToLower(), ct);

        if (exists)
        {
            return Error.Conflict("Category.Duplicate", "Category with this name already exists");
        }

        var category = new Category
        {
            Name = normalizedName,
            UserId = userId
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(ct);

        return CategoryDto.FromEntity(category);
    }

    public async Task<ErrorOr<CategoryDto>> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, ct);

        if (category is null)
        {
            return Error.NotFound("Category.NotFound", "Category not found");
        }

        var normalizedName = request.Name.Trim();
        var nameExists = await dbContext.Categories
            .AnyAsync(x => x.UserId == userId && x.Id != id && x.Name.ToLower() == normalizedName.ToLower(), ct);
        if (nameExists)
        {
            return Error.Conflict("Category.Duplicate", "Category with this name already exists");
        }

        category.Name = normalizedName;
        await dbContext.SaveChangesAsync(ct);

        return CategoryDto.FromEntity(category);
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(
        Guid userId,
        Guid id,
        CancellationToken ct = default)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, ct);

        if (category is null)
        {
            return Error.NotFound("Category.NotFound", "Category not found");
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(ct);

        return Result.Deleted;
    }

    // this function was written by ai
    public async Task<ErrorOr<IReadOnlyList<CategoryDto>>> SeedAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var poolOfCategories = new[]
        {
            "Work", "Personal", "Shopping", "Health", "Finance", "Study",
            "Projects", "Fitness", "Travel", "Home", "Learning", "Side Hustle",
            "Family", "Entertainment", "Errands", "Reading List", "Ideas", "Recipes",
            "Tech", "Events", "Self-Care", "Career", "Hobbies", "Home Improvement"
        };

        var existingCategories = await dbContext.Categories
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        var existingNames = existingCategories
            .Select(x => x.Name.ToLowerInvariant())
            .ToHashSet();

        var candidates = poolOfCategories
            .Where(name => !existingNames.Contains(name.ToLowerInvariant()))
            .Take(6)
            .ToList();

        if (candidates.Count == 0)
        {
            var faker = new Bogus.Faker();
            for (var i = 0; i < 5; i++)
            {
                var department = faker.Commerce.Department();
                var categoryName = department.Length > 50 ? department[..50] : department;
                if (!existingNames.Contains(categoryName.ToLowerInvariant()) && !candidates.Contains(categoryName))
                {
                    candidates.Add(categoryName);
                    existingNames.Add(categoryName.ToLowerInvariant());
                }
            }
        }

        var newCategories = candidates
            .Select(name => new Category
            {
                Name = name,
                UserId = userId
            })
            .ToList();

        if (newCategories.Count > 0)
        {
            dbContext.Categories.AddRange(newCategories);
            await dbContext.SaveChangesAsync(ct);
        }

        var allCategories = await dbContext.Categories
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(CategoryDto.Projection)
            .ToListAsync(ct);

        return allCategories;
    }
}
