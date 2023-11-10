# SECTION 1: Introduction
# SECTION 2: Building a Walking Skeleton Part One - API

**Solution**
Um arquivo de soluções é um recipiente para conter projetos.

COMMAND STEPS

1. C:\Users\medve\source\repos\udemy\DatingApp> dotnet new sln
Cria solução com nome da pasta em que estou.

2. C:\Users\medve\source\repos\udemy\DatingApp> dotnet new webapi -n API

3. C:\Users\medve\source\repos\udemy\DatingApp> dotnet sln -h
dotnet sln add <CAMINHO-DO-PROJETO>
Adiciona projeto à solução.

4. C:\Users\medve\source\repos\udemy\DatingApp> dotnet sln list

C:\Users\medve\source\repos\udemy\DatingApp> ls
API DatingApp.sln
C:\Users\medve\source\repos\udemy\DatingApp> code .

5. No terminal do VSCode:
	C:\Users\medve\source\repos\udemy\DatingApp> cd API/
	C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet run
    C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet dev-certs https –trust (no Windows; no Linux é diferente)

**https://www.nuget.org/**

6. C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet add package Microsoft.EntityFrameworkCore.Sqlite

7. C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet add package Microsoft.EntityFrameworkCore.Design
   Permite usar as migrations.

8. AppUsers
API\Entities\AppUser.cs

namespace API.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; }
}


9. C:\Users\medve\source\repos\udemy\DatingApp\API> mkdir Data

10. C:\Users\medve\source\repos\udemy\DatingApp\API> New-Item -Path .\Data\DataContext.cs

11. 
API\Data\DataContext.cs

using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {        
    }

    public DbSet<AppUser> Users { get; set; }
}

A intância DbContext representa uma sessão com o db e pode ser usada para consultar e salvar instâncias.
Para conexão com db, precisamos fornecer uma string de conexão.

12. 
API\Program.cs

builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

API\appsettings.Development.json

{
  "Logging": {
    ...
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data source=datingapp.db"
  }
}

13. C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet tool list -g
ID do Pacote      Versão      Comandos 
---------------------------------------
dotnet-ef         7.0.11      dotnet-ef

14. C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef migrations -h

15. C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef migrations add InitialCreate -o .\Data\Migrations

16. C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef database -h

17. C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef database update
db criado :)(:

Para ver o db SQLite:
1 - Ctrl Shift p
2 - SQLite: Open Database
3 - API/datingapp.db

Caso queira inserir dados em uma tabela específica, clique com o botão direito sobre o nome da tabela, selecione New Query [Insert].
Insira a(s) query(ies), selecione as linhas que quer executar, clique com o botão direito, escolha Run Selected Query.

18. adição do arquivo .editorconfig na raiz com as configurações que copiei do repositório do Neil. Isso para que quando eu opte por clicar na ajuda do VSCode para criar campos privados nas classes, estes sejam criados com underscore.

19. UsersController
API\Controllers\UsersController.cs

using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DataContext _context;
    public UsersController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<AppUser>> GetUsers()
    {
        var users = _context.Users.ToList();

        return users;
    }

    [HttpGet("{id}")]  // api/users/1
    public ActionResult<AppUser> GetUser(int id)
    {
        return _context.Users.Find(id);
    }
}

20. C:\Users\medve\source\repos\udemy\DatingApp> git init

21. C:\Users\medve\source\repos\udemy\DatingApp> dotnet new list

22. C:\Users\medve\source\repos\udemy\DatingApp> dotnet new gitignore

23. Adicionar appsettings.json na última linha do .gitignore

.gitignore
API/appsettings.json

24. C:\Users\medve\source\repos\udemy\DatingApp> dotnet new globaljson
Gera arquivo global.json na raiz. Especifica qual versão do SDK é usada neste projeto em particular.

25. C:\Users\medve\source\repos\udemy\DatingApp> git push-u origin main


# FIM DA SESSÃO 2

Implementamos funcionalidades de API básicas e estudamos uma introdução à:

1. Uso de dotnet CLI

2. API Controllers e Endpoints
API Controller herda da classe base ControllerBase.
Possui um atributo de controlador de API e um controlador de API possui diferentes endpoints.

3. Entity Framework

4. Estrutura do projeto de API
Classe Program com método Main.

5. Configuração e variáveis de ambiente

6. Código fonte
Criação de repositório remoto no GitHub e vinculação com este projeto local.
