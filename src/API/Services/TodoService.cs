using API.Interfaces;
using API.Models.Common;
using API.Models.Requests;
using API.Models.Responses;
using API.Persistence;
using API.Persistence.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public sealed class TodoService(AppDbContext dbContext) : ITodoService
{
    public async Task<PaginatedResult<TodoDto>> GetListAsync(
        Guid userId,
        GetTodosQuery query,
        CancellationToken ct = default)
    {
        var queryable = dbContext.TodoItems
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(x =>
                x.Title.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (query.CategoryId.HasValue)
        {
            queryable = queryable.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        if (query.IsCompleted.HasValue)
        {
            queryable = queryable.Where(x => x.IsComplete == query.IsCompleted.Value);
        }

        var totalCount = await queryable.CountAsync(ct);

        var items = await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(TodoDto.Projection)
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PaginatedResult<TodoDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<ErrorOr<TodoDto>> GetByIdAsync(
        Guid userId,
        Guid todoId,
        CancellationToken ct = default)
    {
        var todo = await dbContext.TodoItems
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(x => x.Id == todoId && x.UserId == userId, ct);

        return todo is null
            ? Error.NotFound("Todo.NotFound", "Task not found")
            : TodoDto.FromEntity(todo);
    }

    public async Task<ErrorOr<TodoDto>> CreateAsync(
        Guid userId,
        CreateTodoRequest request,
        CancellationToken ct = default)
    {
        if (request.CategoryId.HasValue)
        {
            var categoryExist = await dbContext.Categories
                .AnyAsync(x => x.Id == request.CategoryId.Value && x.UserId == userId, ct);

            if (!categoryExist)
            {
                return Error.NotFound("Category.NotFound", "Category does not exist");
            }
        }

        var todo = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        dbContext.TodoItems.Add(todo);
        await dbContext.SaveChangesAsync(ct);

        return TodoDto.FromEntity(todo);
    }

    public async Task<ErrorOr<TodoDto>> UpdateAsync(
        Guid userId,
        Guid todoId,
        UpdateTodoRequest request,
        CancellationToken ct = default)
    {
        var todo = await dbContext.TodoItems.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == todoId, ct);
        if (todo is null)
        {
            return Error.NotFound("Todo.NotFound", "Task not found");
        }

        if (request.CategoryId.HasValue && request.CategoryId != todo.CategoryId)
        {
            var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == request.CategoryId.Value && x.UserId == userId, ct);
            if (!categoryExists)
            {
                return Error.NotFound("Category.NotFound", "Category does not exits");
            }
        }

        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.IsComplete = request.IsCompleted;
        todo.CategoryId = request.CategoryId;

        await dbContext.SaveChangesAsync(ct);

        return TodoDto.FromEntity(todo);
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(
        Guid userId,
        Guid todoId,
        CancellationToken ct = default)
    {
        var todo = await dbContext.TodoItems.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == todoId, ct);
        if (todo is null)
        {
            return Error.NotFound("Todo.NotFound", "Task not found");
        }

        dbContext.TodoItems.Remove(todo);
        await dbContext.SaveChangesAsync(ct);

        return Result.Deleted;
    }

    // this function was written by ai
    public async Task<ErrorOr<IReadOnlyList<TodoDto>>> SeedAsync(
        Guid userId,
        int count = 10,
        CancellationToken ct = default)
    {
        if (count <= 0) count = 10;
        if (count > 50) count = 50;

        var existingCategories = await dbContext.Categories
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        if (existingCategories.Count == 0)
        {
            var defaultCategoryNames = new[] { "Work", "Personal", "Shopping", "Health", "Finance", "Study" };
            var newCategories = defaultCategoryNames.Select(name => new Category
            {
                Name = name,
                UserId = userId
            }).ToList();

            dbContext.Categories.AddRange(newCategories);
            await dbContext.SaveChangesAsync(ct);
            existingCategories = newCategories;
        }

        var faker = new Bogus.Faker();

        var workGenerators = new Func<Bogus.Faker, (string Title, string Description)>[]
        {
            f => ($"Review PR: {f.Hacker.Verb()} {f.Hacker.Noun()} module", $"Ensure test coverage for the new {f.Hacker.Adjective()} {f.Hacker.Noun()} implementation. Check code formatting and edge cases."),
            f => ($"Fix {f.Hacker.Adjective()} bug in {f.Hacker.Abbreviation()} service", $"Users reported intermittent failure when trying to {f.Hacker.Verb()} {f.Hacker.Noun()}. Check application logs and add regression test."),
            f => ($"Draft RFC for {f.Company.CatchPhrase()}", $"Document architecture design, data flow, migration strategy, and latency requirements."),
            f => ($"Prepare sprint demo slides on {f.Company.Bs()}", $"Include metrics from the last sprint, milestone progress, and team achievements."),
            f => ($"Deploy {f.System.Semver()} release to staging", $"Verify service health metrics, smoke test core endpoints, and check alert logs."),
            f => ($"Refactor {f.Hacker.Verb()} logic in {f.Hacker.Noun()} controller", $"Clean up legacy code, simplify error handling, and improve async throughput.")
        };

        var shoppingGenerators = new Func<Bogus.Faker, (string Title, string Description)>[]
        {
            f => ($"Buy {f.Commerce.ProductMaterial()} {f.Commerce.ProductName()}", $"Compare prices across retailers and check reviews. Quantity needed: {f.Random.Number(1, 4)}."),
            f => ($"Order groceries: {f.Commerce.ProductAdjective()} {f.Commerce.Product()}", $"Also add fresh organic produce, sourdough bread, oat milk, and coffee beans."),
            f => ($"Purchase replacement for {f.Commerce.ProductName()}", $"Look for warranty options and discount codes for {f.Company.CompanyName()}."),
            f => ($"Pick up {f.Commerce.ProductName()} at {f.Address.StreetName()} store", $"Order confirmation number #{f.Random.Number(1000, 9999)}.")
        };

        var personalGenerators = new Func<Bogus.Faker, (string Title, string Description)>[]
        {
            f => ($"Plan weekend trip to {f.Address.City()}", $"Research hotels near {f.Address.StreetName()} and explore hiking trails in the area."),
            f => ($"Call {f.Name.FullName()}", $"Catch up over the phone and discuss upcoming family get-together."),
            f => ($"Organize workspace and clean {f.Address.StreetSuffix()} desk", $"Declutter surface, sort cables, wipe down monitors, and organize stationery."),
            f => ($"Renew {f.Company.CatchPhrase()} annual membership", $"Check renewal terms, verify payment method, and update emergency contact info.")
        };

        var healthGenerators = new Func<Bogus.Faker, (string Title, string Description)>[]
        {
            f => ($"Schedule appointment with Dr. {f.Name.LastName()}", $"Routine wellness checkup and preventive screening at {f.Company.CompanyName()} clinic."),
            f => ($"Complete {f.Random.Number(20, 60)}-minute {f.PickRandom("cardio", "HIIT", "strength training", "yoga")} workout", $"Focus on steady heart rate, form, and post-session stretching."),
            f => ($"Meal prep {f.Commerce.ProductAdjective()} lunches for the week", $"Cook balanced high-protein meals with roasted vegetables and quinoa."),
            f => ($"Track daily hydration ({f.Random.Number(2, 4)}L target)", $"Record water intake throughout the day in fitness tracker.")
        };

        var financeGenerators = new Func<Bogus.Faker, (string Title, string Description)>[]
        {
            f => ($"Review monthly budget and {f.Finance.AccountName()}", $"Audit recurring subscription charges and categorize discretionary spending."),
            f => ($"Pay {f.Company.CompanyName()} invoice", $"Total amount: ${f.Random.Number(35, 290)}. Verify invoice number and due date."),
            f => ($"Transfer ${f.Random.Number(100, 1000)} to {f.Finance.AccountName()}", $"Scheduled monthly automated transfer towards emergency fund."),
            f => ($"Organize tax receipts and financial statements", $"Gather medical deductions, donation receipts, and prepare expense summary sheet.")
        };

        var studyGenerators = new Func<Bogus.Faker, (string Title, string Description)>[]
        {
            f => ($"Read chapter {f.Random.Number(1, 12)} of {f.PickRandom("Designing Data-Intensive Applications", "Clean Code", "Domain-Driven Design", "Concurrency in .NET", "Kubernetes Up & Running")}", $"Write concise summary notes and highlight architectural patterns."),
            f => ($"Complete tutorial on {f.PickRandom("TypeScript 5", "C# 14", "PostgreSQL indexes", "Docker multi-stage builds", "Aspire orchestration")}", $"Build sample playground application to practice concepts."),
            f => ($"Solve {f.Random.Number(2, 4)} LeetCode {f.PickRandom("Easy", "Medium")} problems", $"Practice {f.PickRandom("Dynamic Programming", "Trees & Graphs", "Two Pointers", "Sliding Window")} algorithms."),
            f => ($"Watch conference presentation on {f.Hacker.IngVerb()} {f.Hacker.Noun()}", $"Summarize key insights and create action items for personal development.")
        };

        var newTodos = new List<TodoItem>();

        for (var i = 0; i < count; i++)
        {
            var category = faker.PickRandom(existingCategories);
            var catName = category.Name.ToLowerInvariant();

            (string Title, string Description) taskData;

            if (catName.Contains("work") || catName.Contains("project") || catName.Contains("dev") || catName.Contains("tech"))
            {
                taskData = faker.PickRandom(workGenerators)(faker);
            }
            else if (catName.Contains("shop") || catName.Contains("buy"))
            {
                taskData = faker.PickRandom(shoppingGenerators)(faker);
            }
            else if (catName.Contains("health") || catName.Contains("fit") || catName.Contains("care"))
            {
                taskData = faker.PickRandom(healthGenerators)(faker);
            }
            else if (catName.Contains("finan") || catName.Contains("money") || catName.Contains("bill"))
            {
                taskData = faker.PickRandom(financeGenerators)(faker);
            }
            else if (catName.Contains("stud") || catName.Contains("learn") || catName.Contains("book"))
            {
                taskData = faker.PickRandom(studyGenerators)(faker);
            }
            else if (catName.Contains("person") || catName.Contains("home") || catName.Contains("life"))
            {
                taskData = faker.PickRandom(personalGenerators)(faker);
            }
            else
            {
                var action = faker.PickRandom("Organize", "Update", "Finalize", "Inspect", "Coordinate", "Prepare", "Schedule", "Review");
                taskData = ($"{action} {faker.Company.CatchPhrase()}", $"Details: {faker.Lorem.Paragraph(2)}");
            }

            var title = taskData.Title.Length > 200 ? taskData.Title[..200] : taskData.Title;
            var description = taskData.Description.Length > 1000 ? taskData.Description[..1000] : taskData.Description;

            var todo = new TodoItem
            {
                Title = title,
                Description = description,
                IsComplete = faker.Random.Bool(0.35f),
                UserId = userId,
                CategoryId = category.Id,
                Category = category
            };

            newTodos.Add(todo);
        }

        dbContext.TodoItems.AddRange(newTodos);
        await dbContext.SaveChangesAsync(ct);

        return newTodos.Select(TodoDto.FromEntity).ToList();
    }
}
