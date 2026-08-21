using API.Interfaces;

namespace API.Services;

public static class Extensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        public TBuilder AddServices()
        {
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITodoService, TodoService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            return builder;
        }
    }
}
