using Microsoft.EntityFrameworkCore;

namespace API.Persistence;

public static class Extensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        public TBuilder AddPersistence()
        {
            builder.AddNpgsqlDbContext<AppDbContext>("database");
            
            return builder;
        }
    }

    extension<THost>(THost host) where THost : IHost
    {
        public THost MigrateDatabase()
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
            return host;
        }
    }
}