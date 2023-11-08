using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using API.Entities;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
        /*
            O arquivo Data/UserSeedData.json possui JSON com nomes de chave em PascalCase.
            Ainda que algumas delas esteja em camelCase, a linha acima permitirá que não 
            haja erro.
        */

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

        // gerar a mesma senha (facilitar) para cada usuário na lista
        foreach(var user in users)
        {
            using var hmac = new HMACSHA512();

            user.UserName = user.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
            user.PasswordSalt = hmac.Key;

            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
    }
}
