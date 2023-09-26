using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        services.AddCors();
        services.AddScoped<ITokenService, TokenService>();
        /*
            Poderia parametrizar AddScoped somente com TokenService, sem a interface:
                builder.Services.AddScoped<TokenService>();
            
            Mas usar a interface isola melhor o código e facilita ao implementar testes.
        */

        return services;
    } 
}
