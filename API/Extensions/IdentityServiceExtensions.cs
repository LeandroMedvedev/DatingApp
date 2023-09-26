using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["TokenKey"])
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });
        /*
            1. SERVIÇO ACRESCENTADO PARA TOKEN
            builder.Services.AddAuthentication(...) diz ao servidor como autenticar.
            Dá ao servidor informações suficientes sobre o token para validá-lo com base na chave de assinatura do emissor que implementamos.

            2. MIDDLEWARES ACRESCENTADOS PARA AUTENTICAÇÃO E AUTORIZAÇÃO
            Para que serviço (acima) possa ser utilizado, precisamos acrescentar os middlewares abaixo:

                app.UseAuthentication();
                app.UseAuthorization();

            Devem ser postos após app.UseCors(...) e antes de app.MapControllers();
        */
        return services;
    }
}
