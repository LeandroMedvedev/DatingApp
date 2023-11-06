using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("https://localhost:4200")
);

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();  // dá acesso a todos os serviços dentro da classe Program
var services = scope.ServiceProvider;
/*
    O middleware de tratamento de exceções que criamos, ExceptionMiddleware, trata exceções de requisições HTTP.
    Como não é o caso aqui, precisamos usar try/catch.
*/
try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    /*
        MigrateAsync
        Aplica de forma assíncrona quaisquer migrações pendentes do contexto ao banco de dados. Criará o banco de
        dados se ele ainda não existir. Ou seja, se eu reiniciar a aplicação e a execução do programa perceber que 
        o db não existe, ele será criado, tabelas, colunas, dados.
    */
    await Seed.SeedUsers(context);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();
