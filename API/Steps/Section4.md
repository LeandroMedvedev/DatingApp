# SECTION 4

## Learning Goals

Implement basic authentication in our app and have an understanding of:

1. How store passwords in the Database
2. Using inheritance in C# - DRY
3. Using the C# debugger
4. Using Data Transfer Objects (DTOs)
5. Validation
6. JSON Web Tokens (JWTs)
7. Using services in C#
8. Middleware
9. Extension methods - DRY

Ao ter uma ideia para um app, por onde começar:

### Where do I start?

**Requirements**
Users should be able to log in
Users should be able to register
Users should be able to veiew others users
Users should be able to privately message other users

Repare que users aparece repetidamente.
Comece com entidade User e então construa este aplicativo em torno disso.

### 34. Safe storage of passwords

Option 1 - Storing in clear text -> NO

Option 2 - Hashing the password -> NO
O problema com hash de senha é que se dois usuários criarem a mesma senha ela será hasheada exatamente igual no db. Um invasor poderia ter acesso a duas contas. É muito comum usuários usarem a mesma senha em apps diferentes.
O problema de ter apenas 1 senha é que há online dicionários de todas as possíveis combinações de senhas comumente usadas ou combinações de diferentes senhas disponíveis, e seus hashes já foram calculados em vários algoritmos diferentes. Resumindo: sua senha seria descoberta em poucos segundos.

Option 3 - Hashing and salting the password -> YES
Nesta opção, ainda que o usuário use a mesma senha para vários apps ou a mesma senha de outro usuário, a senha é armazenada em um algoritmo hash diferente no db. Não há como um invasor saber que 2 usuários compartilham a mesma senha.
O salt de senha é outra string aleatória passada ao hash computado que em seguida embaralha o hash de algum modo para torná-lo muito diferente do hash calculado original.

Option 4 - Use ASP.NET Identity -> BEST OPTION

FAQs (Frequently Answered Questions)
Why don't you use ASP.NET Identity?
Faremos isso mais tarde no curso. Ele faz tudo por nós, mas não nos mostra como funciona.

Why are you storing the password salt in the db? Isn't this less secure?

### 35. Updating the user entity

1.  API\Entities\AppUser.cs

namespace API.Entities;

public class AppUser
{
...
public byte[] PasswordHash { get; set; }
public byte[] PasswordSalt { get; set; }
}

2. PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef migrations add UserPasswordAdded
   Não preciso especificar diretório de saída se já houver migrações feitas anteriormente.

3. PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef database update

4. Ctrl Shift p
   SQLite: Open Database
   Enter

### 36. Creating a base API controller

5.  API\Controllers\BaseApiController.cs

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{

}

6.  API\Controllers\UsersController.cs

...
public class UsersController : BaseApiController
{
...
}

### 37. Creating an Account Controller with a register endpoint

7.  API\Controllers\AccountController.cs

using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController : BaseApiController
{
private readonly DataContext \_context;
public AccountController(DataContext context)
{
\_context = context;
}

    [HttpPost("register")]  // api/account/register
    public async Task<ActionResult<AppUser>> Register(string username, string password)
    {
        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            UserName = username,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
            PasswordSalt = hmac.Key,
        };
        _context.Users.Add(user);           // rastreia nova entidade na memória
        await _context.SaveChangesAsync();  // salva no db

        return user;
    }

}

### 38. Using the debugger

### 39. Using DTOs

DTOs -> Data Transfer Objects
Objeto de Transferência de Dados é basicamente um objeto usado para encapsular alguns dados e enviá-los de um subsistema de uma aplicação para outro.
Um DTO permite criar um objeto diferente e retornar somente as propriedades que interessam, omitindo a senha, por exemplo.

8.  API\DTOs\RegisterDto.cs

namespace API.DTOs;

public class RegisterDto
{
public string Username { get; set; }
public string Password { get; set; }
}

9.  API\Controllers\AccountController.cs

using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
...
[HttpPost("register")] // api/account/register
public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
{
if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key,
        };
 
        _context.Users.Add(user);           // rastreia nova entidade na memória
        await _context.SaveChangesAsync();  // salva no db

        return user;
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }

}

### 40. Adding validation

Modos de tornar um campo string obrigatório:

**I**. habilitar <Nullable>enable</Nullable> em API\API.csproj.
**II**. usar [Required] (using System.ComponentModel.DataAnnotations;) sobre o campo na classe. Neste caso precisaria rodar as migrações novamente para validar esta alteração na entidade.
**III**. fazer a validação no DTO com [Required] (using System.ComponentModel.DataAnnotations;) sobre o campo desejado.

10. API\DTOs\RegisterDto.cs

using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
[Required]
public string Username { get; set; }
[Required]
public string Password { get; set; }
}

**OBS** _Não é preciso rodar migração ao alterar uma classe DTO, já que atua somente como um serializador._

### 41. Adding a login endpoint

11. 
API\DTOs\LoginDto.cs

namespace API.DTOs;

public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

12. 
API\Controllers\AccountController.cs

[HttpPost("login")]
public async Task<ActionResult<AppUser>> Login(LoginDto loginDto)
{
    var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

    if (user == null) return Unauthorized("invalid username");

    using var hmac = new HMACSHA512(user.PasswordSalt);

    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

    for (int i = 0; i < computedHash.Length; i++)
    {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
    }

    return user;
}

### 42. JSON web tokens

**JWT**

Padrão da indústria para tokens (RFC 7519)
Independente e pode conter:

* Credenciais
* Declarações (aquilo que o usuário afirma ser; um nome de usuário, por exemplo)
* Outras informações

**Token Authentication**

CLIENT                                                  SERVER
        -> Sends username + password
        Validate credentials and return JWT <-
        -> Sends JWT with further (outras) requests
        Server verifies JWT and sends back response <-
            
**Benefits of JWT**

**I.** No session to manage - JWTs are self contained tokens
**II.** Portátil - um simples token pode ser usado com múltiplos backends
**III.** Não são necessários cookies - compatíveis com dispositivos móveis *celulares não lidam com cookies*
**IV.** Performance - uma vez emitido o token, não é preciso fazer requisições ao db para verificar autenticação do usuário.
Usuário mantém token e, sempre que precisar acessar um recurso protegido, apresenta seu token num cabeçalho de autenticação.


### 43. Adding a token service

13. 
API\Interfaces\ITokenService.cs

using API.Entities;

namespace API.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
}

14. 
API\Services\TokenService.cs

using API.Entities;
using API.Interfaces;

namespace API.Services;

public class TokenService : ITokenService
{
    public string CreateToken(AppUser user)
    {
        throw new NotImplementedException();
    }
}

15. 
API\Program.cs

...
builder.Services.AddScoped<ITokenService, TokenService>();
...


### 44. Adding the create token logic

16. PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.0

17. 
API\Services\TokenService.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

    public string CreateToken(AppUser user)
    {        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.UserName)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}


### 45. Creating a User DTO and returning the token

18. 
API\DTOs\UserDto.cs

namespace API.DTOs;

public class UserDto
{
    public string Username { get; set; }
    public string Token { get; set; }
}

19. 
API\Controllers\AccountController.cs

...

public class AccountController : BaseApiController
{
    ...
    private readonly ITokenService _tokenService;

    public AccountController(DataContext context, ITokenService tokenService)
    {
        ...
        _tokenService = tokenService;
    }

    [HttpPost("register")]  // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        ...
        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]  // api/account/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        ...
        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }
    ...
}

20. 
API\appsettings.Development.json

{
  ...
  "TokenKey": "BXRiQflQewK6sSUNPsA8dx+NX6e24D5sMOPYl+yM6FvlnI+DO6EFHMya9k6lFQZK"
}

**OBS:** gerar chave aleatória base 64 no WSL com 64 caracteres:
                leandro@DESKTOP-F9I1L20:/mnt/c/Users/medve$ openssl rand -base64 48


### 46. Adding the authentication middleware

21. 
API\Controllers\UsersController.cs

...
[Authorize]
public class UsersController : BaseApiController
{
    ...
    [AllowAnonymous]
    [HttpGet]  // api/users
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        return users;
    }
    ...
}

22. PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 7.0.11

23. 
API\Program.cs

...
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["TokenKey"])
        ),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});
...

app.UseAuthentication();
app.UseAuthorization();
/*
    app.UseAuthentication();
    app.UseAuthorization();
    Devem ser postos após app.UseCors(...) e antes de app.MapControllers();
*/
...

### 47. Adding extension methods

24.  
API\Program.cs

builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();
builder.Services.AddScoped<ITokenService, TokenService>();

*Recorto linhas acima em Program.cs e colo no método de extensão que criei, com algumas adaptações:*
 
API\Extensions\ApplicationServiceExtensions.cs

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

        return services;
    }
}

25. 
API\Program.cs

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["TokenKey"])
        ),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

Recorto linhas acima de Program.cs e colo em API\Extensions\IdentityServiceExtensions.cs, com algumas adaptações.

API\Extensions\IdentityServiceExtensions.cs

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

        return services;
    }
}

26. 
API\Program.cs
...
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
...

27. PS C:\Users\medve\source\repos\udemy\DatingApp\API> git add .

28. PS C:\Users\medve\source\repos\udemy\DatingApp\API> git commit -m 'End of section 4'





### 48. Section 4 summary
