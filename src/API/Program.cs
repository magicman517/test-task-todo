using System.Reflection;
using API.Auth;
using API.Persistence;
using API.Services;
using FluentValidation;
using Scalar.AspNetCore;
using ServiceDefaults;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddPersistence();

if (args.Contains("--migrate"))
{
    var migratorApp = builder.Build();
    migratorApp.MigrateDatabase();
    return;
}

builder.AddAuthentication();
builder.AddServices();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi("/api/reference/{documentName}.json");
app.MapScalarApiReference("/api/reference", options =>
{
    options.AddDocument("v1", "Todo API", "/api/reference/{documentName}.json");

    options
        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
        .DisableAgent()
        .DisableMcp()
        .DisableTelemetry()
        .HideClientButton()
        .HideDocumentDownload()
        .HideModels();
});

app.MapControllers();

app.MapFallbackToFile("index.html");

await app.RunAsync();
